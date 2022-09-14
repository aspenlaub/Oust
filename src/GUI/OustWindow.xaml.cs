using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Web;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.GUI;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Helpers;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Autofac;
using IContainer = Autofac.IContainer;
using WindowsApplication = System.Windows.Application;

[assembly: InternalsVisibleTo("Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test")]
namespace Aspenlaub.Net.GitHub.CSharp.Oust.GUI;

/// <summary>
/// Interaction logic for OustWindow.xaml
/// </summary>
// ReSharper disable once UnusedMember.Global
public partial class OustWindow : IAsyncDisposable {
    private const string WebViewLoaderDll = "WebView2Loader.dll";

    private static IContainer Container { get; set; }

    private IApplication _OustApp;
    private TashTimer<ApplicationModel> _TashTimer;

    private IExtractSubScriptPopup _ExtractSubScriptPopup;
    private readonly IProgressWindow _ProgressWindow;
    private readonly EnvironmentType _EnvironmentType;

    public OustWindow() : this(Context.DefaultEnvironmentType) { }

    public OustWindow(EnvironmentType environmentType) {
        InitializeComponent();

        _EnvironmentType = environmentType;
        _ProgressWindow = new ProgressWindow();

        Title = environmentType == EnvironmentType.UnitTest ? Properties.Resources.OustUnitTestWindowTitle : Properties.Resources.OustWindowTitle;

        Name = environmentType == EnvironmentType.UnitTest ? Properties.Resources.OustUnitTestWindowName : Properties.Resources.OustWindowName;
        AutomationProperties.SetAutomationId(this, Name);
        AutomationProperties.SetName(this, Name);
    }

    public async ValueTask DisposeAsync() {
        if (_TashTimer == null) { return; }

        await _TashTimer.StopTimerAndConfirmDeadAsync(false);
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e) {
        var contextFactory = new ContextFactory();
        await using var db = await contextFactory.CreateAsync(Context.DefaultEnvironmentType);
        db.Migrate();

        await BuildContainerIfNecessaryAsync();

        var folder = GetType().Assembly.Location;
        folder = folder.Substring(0, folder.LastIndexOf(@"\", StringComparison.Ordinal));
        var file = Directory.GetFiles(folder, WebViewLoaderDll, SearchOption.AllDirectories).FirstOrDefault();
        if (string.IsNullOrEmpty(file)) {
            MessageBox.Show($"Missing {WebViewLoaderDll}", nameof(Window), MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        var httpGate = Container.Resolve<IHttpGate>();
        var localhostAvailable = await httpGate.IsLocalHostAvailableAsync();
        if (!localhostAvailable) {
            MessageBox.Show(Properties.Resources.LocalHostNotAvailable, Properties.Resources.OustWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);
        }

        var result = await httpGate.GetAsync(new Uri(await CleanWindowIdCookieUrlAsync()));
        if (result.StatusCode != HttpStatusCode.OK || !(await result.Content.ReadAsStringAsync()).Contains("OK")) {
            MessageBox.Show(Properties.Resources.CouldNotClearWindowIdCookie, Properties.Resources.OustWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);
        }

        var processes = Process.GetProcesses().Where(p => p.ProcessName.ToUpper().Contains("SQLSERVR")).ToList();
        if (!processes.Any()) {
            MessageBox.Show(Properties.Resources.SqlServerNotAvailable, Properties.Resources.OustWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);
        }

        var errorsAndInfos = new ErrorsAndInfos();
        await Container.Resolve<IDumpFolderChecker>().CheckDumpFolderAsync(errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            MessageBox.Show(string.Join("\r\n", errorsAndInfos.Errors));
            Environment.Exit(0);
        }

        await Container.Resolve<IImporter>().ImportAsync(errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            MessageBox.Show(string.Join("\r\n", errorsAndInfos.Errors));
            Environment.Exit(0);
        }

        _OustApp = Container.Resolve<IApplication>(new NamedParameter("extractSubScriptPopup", _ExtractSubScriptPopup), new NamedParameter("progressWindow", _ProgressWindow));
        await _OustApp.OnLoadedAsync();

        var guiToAppGate = Container.Resolve<IGuiToWebViewApplicationGate>();
        var buttonNameToCommandMapper = Container.Resolve<IButtonNameToCommandMapper>();

        guiToAppGate.WireWebView(WebView);

        var commands = _OustApp.Commands;
        guiToAppGate.WireButtonAndCommand(AddOrReplaceStep, commands.AddOrReplaceStepCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(CodeCoverage, commands.CodeCoverageCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(Consolidate, commands.ConsolidateCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(DeleteStep, commands.DeleteStepCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(ExtractSubScript, commands.ExtractSubScriptCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(MoveUp, commands.MoveUpStepCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(Play, commands.PlayCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(RenameScript, commands.RenameCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(DuplicateScript, commands.DuplicateCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(ShowExecutionStack, commands.ShowExecutionStackCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(StepInto, commands.StepIntoCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(StepOver, commands.StepOverCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(StopCodeCoverage, commands.StopCodeCoverageCommand, buttonNameToCommandMapper);
        guiToAppGate.WireButtonAndCommand(SelectScriptFromList, commands.SelectScriptFromListCommand, buttonNameToCommandMapper);

        var handlers = _OustApp.Handlers;
        guiToAppGate.RegisterAsyncTextBoxCallback(NewScriptName, t => _OustApp.NewScriptNameChangedAsync(t));
        guiToAppGate.RegisterAsyncTextBoxCallback(FormOrIdOrClassInstanceNumber, t => handlers.FormOrControlOrIdOrClassHandler.FormOrIdOrClassInstanceNumberChangedAsync(t));
        guiToAppGate.RegisterAsyncTextBoxCallback(FreeText, t => _OustApp.FreeTextChangedAsync(t));

        guiToAppGate.RegisterAsyncSelectorCallback(ScriptSteps, i => handlers.ScriptStepSelectorHandler.ScriptStepsSelectedIndexChangedAsync(i, false));
        guiToAppGate.RegisterAsyncSelectorCallback(SelectedScript, i => handlers.ScriptSelectorHandler.SelectedScriptSelectedIndexChangedAsync(i, true));
        guiToAppGate.RegisterAsyncSelectorCallback(ScriptStepType, i => handlers.ScriptStepTypeSelectorHandler.ScriptStepTypeSelectedIndexChangedAsync(i, true));
        guiToAppGate.RegisterAsyncSelectorCallback(FormOrControlOrIdOrClass, i => handlers.FormOrControlOrIdOrClassHandler.FormOrControlOrIdOrClassSelectedIndexChangedAsync(i, false));
        guiToAppGate.RegisterAsyncSelectorCallback(SubScript, i => handlers.SubScriptSelectorHandler.SubScriptSelectedIndexChangedAsync(i, false));
        guiToAppGate.RegisterAsyncSelectorCallback(SelectedValue, i => handlers.SelectedValueSelectorHandler.SelectedValueSelectedIndexChangedAsync(i, false));

        try {
            await _OustApp.OnWebViewWiredAsync();

            _TashTimer = new TashTimer<ApplicationModel>(Container.Resolve<ITashAccessor>(), _OustApp.TashHandler, guiToAppGate);
            if (!await _TashTimer.ConnectAndMakeTashRegistrationReturnSuccessAsync(Properties.Resources.OustWindowTitle)) {
                Close();
            }

            _TashTimer.CreateAndStartTimer(_OustApp.CreateTashTaskHandlingStatus());

            await ExceptionHandler.RunAsync(WindowsApplication.Current, TimeSpan.FromSeconds(7));
        } catch (Exception exception) {
            var exceptionLogFolder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubExceptions");
            ExceptionSaver.SaveUnhandledException(exceptionLogFolder, exception, "Oust", _ => {});
            MessageBox.Show(this, Properties.Resources.ExceptionWasLogged, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    private async void OnOustWindowClosingAsync(object sender, CancelEventArgs e) {
        var window = (Window)sender;
        e.Cancel = true;
        window.IsEnabled = false;
        await Task.Yield();

        var errorsAndInfos = new ErrorsAndInfos();
        await Container.Resolve<IDumper>().DumpAsync(errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            window.IsEnabled = true;
            MessageBox.Show(string.Join("\r\n", errorsAndInfos.Errors));
            return;
        }

        Process.GetProcessesByName("Aspenlaub.Net.GitHub.CSharp.Paleface").ToList().ForEach(p => p.Kill());

        if (_TashTimer != null) {
            await _TashTimer.StopTimerAndConfirmDeadAsync(false);
        }

        _ExtractSubScriptPopup.OnApplicationShutdown();
        _ProgressWindow.OnApplicationShutdown();

        WindowsApplication.Current.Shutdown();
    }

    private async Task BuildContainerIfNecessaryAsync() {
        var builder = (await new ContainerBuilder().RegisterForOustApplicationAsync()).RegisterForOustWindow(_EnvironmentType, this);
        Container = builder.Build();
        _ExtractSubScriptPopup = new ExtractSubScriptPopup(_EnvironmentType, Container.Resolve<INewScriptNameValidator>());
    }

    private async Task<string> CleanWindowIdCookieUrlAsync() {
        var logicalUrlRepository = Container.Resolve<ILogicalUrlRepository>();
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await logicalUrlRepository.GetUrlAsync("ClearWinIdCookie", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }

        return url;
    }


}