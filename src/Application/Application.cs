using System.Diagnostics;
using System.Runtime.CompilerServices;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Ouco;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Web;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Application;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Extensions;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

[assembly: InternalsVisibleTo("Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test")]
namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application;

public class Application : WebViewApplicationBase<IGuiAndApplicationSynchronizer, ApplicationModel>, IGuiAndAppHandler, IApplication {
    private IDictionary<ScriptStepType, IScriptStepLogic> _ScriptStepLogicDictionary;
    public IApplicationHandlers Handlers { get; private set; }
    public IApplicationCommands Commands { get; private set; }
    public ITashHandler<ApplicationModel> TashHandler { get; private set; }

    private readonly IDumperNameConverter _DumperNameConverter;
    private readonly ITashAccessor _TashAccessor;
    private readonly ISecuredHttpGate _SecuredHttpGate;
    private readonly IWampLogScanner _WampLogScanner;
    private readonly IScriptAndSubScriptsConsolidator _ScriptAndSubScriptsConsolidator;
    private readonly ISecretRepository _SecretRepository;
    private readonly IContextFactory _ContextFactory;
    private readonly IObsoleteScriptChecker _ObsoleteScriptChecker;
    private readonly IShowExecutionStackPopupFactory _ExecutionStackPopupFactory;
    private readonly IExecutionStackFormatter _ExecutionStackFormatter;
    private readonly IFolderResolver _FolderResolver;
    private readonly IFileDialogTrickster _FileDialogTrickster;
    private readonly IExtractSubScriptPopup _ExtractSubScriptPopup;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;
    private readonly IProgressWindow _ProgressWindow;
    private readonly ILogicalUrlRepository _LogicalUrlRepository;
    private readonly ISelectScriptFromListPopupFactory _SelectScriptFromListPopupFactory;

    public Application(IButtonNameToCommandMapper buttonNameToCommandMapper, IToggleButtonNameToHandlerMapper toggleButtonNameToHandlerMapper,
            IGuiAndApplicationSynchronizer guiAndApplicationSynchronizer, ApplicationModel model, ISimpleLogger simpleLogger,
            ITashAccessor tashAccessor, ISecuredHttpGate securedHttpGate, IWampLogScanner wampLogScanner,
            IScriptAndSubScriptsConsolidator scriptAndSubScriptsConsolidator, IDumperNameConverter dumperNameConverter,
            IObsoleteScriptChecker obsoleteScriptChecker, ISecretRepository secretRepository, IContextFactory contextFactory,
            IExecutionStackFormatter executionStackFormatter, IShowExecutionStackPopupFactory executionStackPopupFactory, IFolderResolver folderResolver,
            IFileDialogTrickster fileDialogTrickster, IExtractSubScriptPopup extractSubScriptPopup, IOustScriptStatementFactory oustScriptStatementFactory,
            IProgressWindow progressWindow, IOucidLogAccessor oucidLogAccessor, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor,
            ILogicalUrlRepository logicalUrlRepository, ISelectScriptFromListPopupFactory selectScriptFromListPopupFactory)
        : base(buttonNameToCommandMapper, toggleButtonNameToHandlerMapper, guiAndApplicationSynchronizer, model, simpleLogger, methodNamesFromStackFramesExtractor, oucidLogAccessor) {
        _DumperNameConverter = dumperNameConverter;
        _TashAccessor = tashAccessor;
        _SecuredHttpGate = securedHttpGate;
        _WampLogScanner = wampLogScanner;
        _ScriptAndSubScriptsConsolidator = scriptAndSubScriptsConsolidator;
        _SecretRepository = secretRepository;
        _ObsoleteScriptChecker = obsoleteScriptChecker;
        _ContextFactory = contextFactory;
        _ExecutionStackFormatter = executionStackFormatter;
        _ExecutionStackPopupFactory = executionStackPopupFactory;
        _FolderResolver = folderResolver;
        _FileDialogTrickster = fileDialogTrickster;
        _ExtractSubScriptPopup = extractSubScriptPopup;
        _OustScriptStatementFactory = oustScriptStatementFactory;
        _ProgressWindow = progressWindow;
        _LogicalUrlRepository = logicalUrlRepository;
        _SelectScriptFromListPopupFactory = selectScriptFromListPopupFactory;
    }

    protected override void CreateCommandsAndHandlers() {
        var oucoHelper = new OucoHelper(Model, this, SimpleLogger, _FolderResolver, _OustScriptStatementFactory, MethodNamesFromStackFramesExtractor);
        var oustSettingsHelper = new OustSettingsHelper(_SecretRepository);
        _ScriptStepLogicDictionary = new Dictionary<ScriptStepType, IScriptStepLogic> {
            { ScriptStepType.Check, new CheckOrUncheckStep(Model, SimpleLogger, this, oucoHelper, _OustScriptStatementFactory, ScriptStepType.Check) },
            { ScriptStepType.CheckSingle, new CheckOrUncheckSingleStep(Model, SimpleLogger, this, _OustScriptStatementFactory, ScriptStepType.CheckSingle) },
            { ScriptStepType.EndOfScript, new EndOfScriptStep(Model) },
            { ScriptStepType.GoToUrl, new GoToUrlStep(Model, SimpleLogger, this, _WampLogScanner, _SecuredHttpGate, _SecretRepository, MethodNamesFromStackFramesExtractor, false) },
            { ScriptStepType.Input, new InputStep(Model, SimpleLogger, this, oucoHelper, _FileDialogTrickster, _OustScriptStatementFactory, oustSettingsHelper) },
            { ScriptStepType.InputIntoSingle, new InputIntoSingleStep(Model, SimpleLogger, this, _OustScriptStatementFactory) },
            { ScriptStepType.InvokeUrl, new GoToUrlStep(Model, SimpleLogger, this, _WampLogScanner, _SecuredHttpGate, _SecretRepository, MethodNamesFromStackFramesExtractor, true) },
            { ScriptStepType.NotExpectedIdOrClass, new NotExpectedIdOrClassStep(Model, SimpleLogger, this, oucoHelper, _OustScriptStatementFactory) },
            { ScriptStepType.NotExpectedContents, new RecognizeOrNotExpectedContentsStep(Model, SimpleLogger, this, oucoHelper, _OustScriptStatementFactory, ScriptStepType.NotExpectedContents) },
            { ScriptStepType.NotExpectedSelection, new RecognizeOrNotExpectedContentsStep(Model, SimpleLogger, this, oucoHelper, _OustScriptStatementFactory, ScriptStepType.NotExpectedSelection) },
            { ScriptStepType.Press, new PressStep(Model, SimpleLogger, this, oucoHelper, _WampLogScanner, _OustScriptStatementFactory) },
            { ScriptStepType.PressSingle, new PressSingleStep(Model, SimpleLogger, this, _WampLogScanner, _OustScriptStatementFactory) },
            { ScriptStepType.Recognize, new RecognizeOrNotExpectedContentsStep(Model, SimpleLogger, this, oucoHelper, _OustScriptStatementFactory, ScriptStepType.Recognize) },
            { ScriptStepType.RecognizeSelection, new RecognizeOrNotExpectedContentsStep(Model, SimpleLogger, this, oucoHelper, _OustScriptStatementFactory, ScriptStepType.RecognizeSelection) },
            { ScriptStepType.Select, new SelectStep(Model, SimpleLogger, this, oucoHelper, _OustScriptStatementFactory, _WampLogScanner) },
            { ScriptStepType.SubScript, new SubScriptStep(Model) },
            { ScriptStepType.Uncheck, new CheckOrUncheckStep(Model, SimpleLogger, this, oucoHelper, _OustScriptStatementFactory, ScriptStepType.Uncheck) },
            { ScriptStepType.UncheckSingle, new CheckOrUncheckSingleStep(Model, SimpleLogger, this, _OustScriptStatementFactory, ScriptStepType.UncheckSingle) },
            { ScriptStepType.WaitAMinute, new WaitAMinuteStep(Model) },
            { ScriptStepType.WaitTenSeconds, new WaitTenSecondsStep(Model) },
            { ScriptStepType.With, new WithStep(Model, SimpleLogger, this, oucoHelper, _OustScriptStatementFactory) },
            { ScriptStepType.WithIdOrClass, new WithIdOrClassStep(Model, SimpleLogger, this, oucoHelper, _OustScriptStatementFactory) },
            { ScriptStepType.RecognizeOkay, new RecognizeOkayStep(Model, _OustScriptStatementFactory, this) },
        };

        var selectedValueSelectorHandler = new SelectedValueSelectorHandler(Model, oucoHelper, this);
        var subScriptSelectorHandler = new SubScriptSelectorHandler(Model, this, _ContextFactory);
        var formOrControlOrIdOrClassHandler = new FormOrControlOrIdOrClassHandler(Model, this, selectedValueSelectorHandler, _ScriptStepLogicDictionary);
        var scriptStepTypeSelectorHandler = new ScriptStepTypeSelectorHandler(Model, this, formOrControlOrIdOrClassHandler, selectedValueSelectorHandler, subScriptSelectorHandler);
        var scriptStepSelectorHandler = new ScriptStepSelectorHandler(Model, this, _ContextFactory, scriptStepTypeSelectorHandler, formOrControlOrIdOrClassHandler,
                subScriptSelectorHandler, selectedValueSelectorHandler, SimpleLogger);
        var scriptSelectorHandler = new ScriptSelectorHandler(Model, scriptStepSelectorHandler, this, _ContextFactory);
        Handlers = new ApplicationHandlers {
            ScriptSelectorHandler = scriptSelectorHandler,
            ScriptStepSelectorHandler = scriptStepSelectorHandler,
            ScriptStepTypeSelectorHandler = scriptStepTypeSelectorHandler,
            SelectedValueSelectorHandler = selectedValueSelectorHandler,
            SubScriptSelectorHandler = subScriptSelectorHandler,
            FormOrControlOrIdOrClassHandler = formOrControlOrIdOrClassHandler
        };

        var stepIntoCommand = new StepIntoCommand(Model, Handlers.ScriptSelectorHandler, Handlers.ScriptStepSelectorHandler, this, SimpleLogger,
                _ScriptStepLogicDictionary, oucoHelper, MethodNamesFromStackFramesExtractor);
        var stepOverCommand = new StepOverCommand(Model, stepIntoCommand);
        Commands = new ApplicationCommands {
            AddOrReplaceStepCommand = new AddOrReplaceStepCommand(Model, Handlers.ScriptStepSelectorHandler, this, stepIntoCommand, _ScriptStepLogicDictionary, _ContextFactory),
            CodeCoverageCommand = new CodeCoverageCommand(this, _LogicalUrlRepository),
            ConsolidateCommand = new ConsolidateCommand(Model, _ScriptAndSubScriptsConsolidator, _ProgressWindow),
            DeleteStepCommand = new DeleteStepCommand(Model, Handlers.ScriptStepSelectorHandler, this, _ContextFactory),
            ExtractSubScriptCommand = new ExtractSubScriptCommand(Model, _ExtractSubScriptPopup, new SubScriptExtractor(_ContextFactory), Handlers.ScriptSelectorHandler, this),
            MoveUpStepCommand = new MoveUpStepCommand(Model, Handlers.ScriptStepSelectorHandler, this, _ContextFactory),
            PlayCommand = new PlayCommand(Model, stepOverCommand),
            RenameCommand = new RenameCommand(Model, Handlers.ScriptSelectorHandler, this, new NewScriptNameValidator(_ContextFactory), _ContextFactory),
            DuplicateCommand = new DuplicateCommand(Model, Handlers.ScriptSelectorHandler, this, new NewScriptNameValidator(_ContextFactory), _ContextFactory),
            ShowExecutionStackCommand = new ShowExecutionStackCommand(Model, _ExecutionStackPopupFactory, _ExecutionStackFormatter),
            StepIntoCommand = stepIntoCommand,
            StepOverCommand = stepOverCommand,
            StopCodeCoverageCommand = new StopCodeCoverageCommand(Model, _DumperNameConverter, this, _LogicalUrlRepository),
            SelectScriptFromListCommand = new SelectScriptFromListCommand(Model, _SelectScriptFromListPopupFactory, scriptSelectorHandler)
        };

        var selectors = new Dictionary<string, ISelector> {
            { nameof(IApplicationModel.SelectedScript), Model.SelectedScript }, { nameof(IApplicationModel.ScriptSteps), Model.ScriptSteps },
            { nameof(IApplicationModel.ScriptStepType), Model.ScriptStepType }, { nameof(IApplicationModel.SelectedValue), Model.SelectedValue },
            { nameof(IApplicationModel.FormOrControlOrIdOrClass), Model.FormOrControlOrIdOrClass }, { nameof(IApplicationModel.SubScript), Model.SubScript }
        };

        var communicator = new TashCommunicator(_TashAccessor, this, SimpleLogger, MethodNamesFromStackFramesExtractor);
        var selectorHandler = new TashSelectorHandler(Handlers, _ObsoleteScriptChecker, SimpleLogger, communicator, selectors, MethodNamesFromStackFramesExtractor);
        var verifyAndSetHandler = new TashVerifyAndSetHandler(SimpleLogger, selectorHandler, communicator, MethodNamesFromStackFramesExtractor, selectors);
        TashHandler = new TashHandler(_TashAccessor, SimpleLogger, ButtonNameToCommandMapper, ToggleButtonNameToHandlerMapper, this, verifyAndSetHandler,
            selectorHandler, communicator, MethodNamesFromStackFramesExtractor);
    }

    public override async Task OnLoadedAsync() {
        await base.OnLoadedAsync();
        await Handlers.ScriptSelectorHandler.EnsureNewScriptAsync();
        await Handlers.ScriptSelectorHandler.UpdateSelectableScriptsAsync();
        Handlers.ScriptStepTypeSelectorHandler.UpdateSelectableScriptStepTypes();

        var errorsAndInfos = new ErrorsAndInfos();
        var oustUtilitiesUrl = await _LogicalUrlRepository.GetUrlAsync("OustUtilitiesJs", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        var jQueryUrl = await _LogicalUrlRepository.GetUrlAsync("JQuery", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        Model.WebView.OnDocumentLoaded.AppendStatement(
            "var script = document.createElement(\"script\"); "
            + $"script.src = \"{oustUtilitiesUrl}\"; "
            + "document.head.appendChild(script);"
            + "if (typeof($jq) == 'undefined') {"
            + "var script = document.createElement(\"script\"); "
            + $"script.src = \"{jQueryUrl}\"; "
            + "document.head.appendChild(script);"
            + "}"
        );
    }

    public async Task OnWebViewWiredAsync() {
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(OnWebViewWiredAsync)))) {
            var scriptStatement = new ScriptStatement {
                Statement = "(function() { " + Model.WebView.OnDocumentLoaded.Statement + " })()"
            };
            await RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        }
    }

    public async Task FreeTextChangedAsync(string text) {
        if (Model.FreeText.Text == text) { return; }

        Model.FreeText.Text = text;
        await EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }

    public async Task NewScriptNameChangedAsync(string text) {
        if (Model.NewScriptName.Text == text) { return; }

        Model.NewScriptName.Text = text;
        await EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }

    public async Task UpdateFreeCodeLabelTextAsync() {
        var stepType = Model.ScriptStepType.SelectionMade ? (ScriptStepType)int.Parse(Model.ScriptStepType.SelectedItem.Guid) : ScriptStepType.EndOfScript;
        Model.FreeText.LabelText = _ScriptStepLogicDictionary[stepType].FreeCodeLabelText;
        Model.FreeText.Enabled = Model.FreeText.LabelText != Properties.Resources.FreeTextTitle;
        if (!Model.FreeText.Enabled) {
            await FreeTextChangedAsync("");
        }
    }

    protected override async Task EnableOrDisableButtonsAsync() {
        Model.AddOrReplaceStep.Enabled = await Commands.AddOrReplaceStepCommand.ShouldBeEnabledAsync();
        Model.CodeCoverage.Enabled = await Commands.CodeCoverageCommand.ShouldBeEnabledAsync();
        Model.Consolidate.Enabled = await Commands.ConsolidateCommand.ShouldBeEnabledAsync();
        Model.Delete.Enabled = await Commands.DeleteStepCommand.ShouldBeEnabledAsync();
        Model.ExtractSubScript.Enabled = await Commands.ExtractSubScriptCommand.ShouldBeEnabledAsync();
        Model.MoveUp.Enabled = await Commands.MoveUpStepCommand.ShouldBeEnabledAsync();
        Model.Play.Enabled = await Commands.PlayCommand.ShouldBeEnabledAsync();
        Model.RenameScript.Enabled = await Commands.RenameCommand.ShouldBeEnabledAsync();
        Model.DuplicateScript.Enabled = await Commands.DuplicateCommand.ShouldBeEnabledAsync();
        Model.ShowExecutionStack.Enabled = await Commands.ShowExecutionStackCommand.ShouldBeEnabledAsync();
        Model.StepInto.Enabled = await Commands.StepIntoCommand.ShouldBeEnabledAsync();
        Model.StepOver.Enabled = await Commands.StepOverCommand.ShouldBeEnabledAsync();
        Model.StopCodeCoverage.Enabled = await Commands.StopCodeCoverageCommand.ShouldBeEnabledAsync();
    }

    public ITashTaskHandlingStatus<ApplicationModel> CreateTashTaskHandlingStatus() {
        return new TashTaskHandlingStatus<ApplicationModel>(Model, Process.GetCurrentProcess().Id);
    }
}