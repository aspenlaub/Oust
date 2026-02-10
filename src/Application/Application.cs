using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Outrap;
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

public class Application(IButtonNameToCommandMapper buttonNameToCommandMapper,
            IToggleButtonNameToHandlerMapper toggleButtonNameToHandlerMapper,
            IGuiAndApplicationSynchronizer guiAndApplicationSynchronizer, ApplicationModel model,
            ISimpleLogger simpleLogger, ITashAccessor tashAccessor, ISecuredHttpGate securedHttpGate,
            IWampLogScanner wampLogScanner, IScriptAndSubScriptsConsolidator scriptAndSubScriptsConsolidator,
            IDumperNameConverter dumperNameConverter, IObsoleteScriptChecker obsoleteScriptChecker,
            ISecretRepository secretRepository, IContextFactory contextFactory,
            IExecutionStackFormatter executionStackFormatter, IShowExecutionStackPopupFactory executionStackPopupFactory,
            IFolderResolver folderResolver, IFileDialogTrickster fileDialogTrickster,
            IExtractSubScriptPopup extractSubScriptPopup, IOustScriptStatementFactory oustScriptStatementFactory,
            IProgressWindow progressWindow, IOucidLogAccessor oucidLogAccessor,
            IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor,
            ILogicalUrlRepository logicalUrlRepository, ISelectScriptFromListPopupFactory selectScriptFromListPopupFactory)
                : WebViewApplicationBase<IGuiAndApplicationSynchronizer, ApplicationModel>(buttonNameToCommandMapper,
                      toggleButtonNameToHandlerMapper, guiAndApplicationSynchronizer, model,
                      simpleLogger, methodNamesFromStackFramesExtractor, oucidLogAccessor),
                    IGuiAndAppHandler, IApplication {
    private IDictionary<ScriptStepType, IScriptStepLogic> _ScriptStepLogicDictionary;
    public IApplicationHandlers Handlers { get; private set; }
    public IApplicationCommands Commands { get; private set; }
    public ITashHandler<ApplicationModel> TashHandler { get; private set; }

    protected override void CreateCommandsAndHandlers() {
        var outrapHelper = new OutrapHelper(Model, this, SimpleLogger, folderResolver, oustScriptStatementFactory, MethodNamesFromStackFramesExtractor);
        var oustSettingsHelper = new OustSettingsHelper(secretRepository);
        _ScriptStepLogicDictionary = new Dictionary<ScriptStepType, IScriptStepLogic> {
            { ScriptStepType.Check, new CheckOrUncheckStep(Model, this, outrapHelper, oustScriptStatementFactory, wampLogScanner, ScriptStepType.Check) },
            { ScriptStepType.CheckSingle, new CheckOrUncheckSingleStep(Model, this, oustScriptStatementFactory, wampLogScanner, ScriptStepType.CheckSingle) },
            { ScriptStepType.EndOfScript, new EndOfScriptStep(Model) },
            { ScriptStepType.GoToUrl, new GoToUrlStep(Model, SimpleLogger, this, wampLogScanner, securedHttpGate, secretRepository, MethodNamesFromStackFramesExtractor, false) },
            { ScriptStepType.Input, new InputStep(ScriptStepType.Input, Model, SimpleLogger, this, outrapHelper, fileDialogTrickster, oustScriptStatementFactory, oustSettingsHelper) },
            { ScriptStepType.InputIntoSingle, new InputIntoSingleStep(Model, SimpleLogger, this, oustScriptStatementFactory) },
            { ScriptStepType.InvokeUrl, new GoToUrlStep(Model, SimpleLogger, this, wampLogScanner, securedHttpGate, secretRepository, MethodNamesFromStackFramesExtractor, true) },
            { ScriptStepType.NotExpectedIdOrClass, new NotExpectedIdOrClassStep(Model, SimpleLogger, this, outrapHelper, oustScriptStatementFactory) },
            { ScriptStepType.NotExpectedContents, new RecognizeOrNotExpectedContentsStep(Model, SimpleLogger, this, outrapHelper, oustScriptStatementFactory, ScriptStepType.NotExpectedContents) },
            { ScriptStepType.NotExpectedSelection, new RecognizeOrNotExpectedContentsStep(Model, SimpleLogger, this, outrapHelper, oustScriptStatementFactory, ScriptStepType.NotExpectedSelection) },
            { ScriptStepType.Press, new PressStep(Model, SimpleLogger, this, outrapHelper, wampLogScanner, oustScriptStatementFactory) },
            { ScriptStepType.PressSingle, new PressSingleStep(Model, SimpleLogger, this, wampLogScanner, oustScriptStatementFactory) },
            { ScriptStepType.Recognize, new RecognizeOrNotExpectedContentsStep(Model, SimpleLogger, this, outrapHelper, oustScriptStatementFactory, ScriptStepType.Recognize) },
            { ScriptStepType.RecognizeSelection, new RecognizeOrNotExpectedContentsStep(Model, SimpleLogger, this, outrapHelper, oustScriptStatementFactory, ScriptStepType.RecognizeSelection) },
            { ScriptStepType.Select, new SelectStep(Model, SimpleLogger, this, outrapHelper, oustScriptStatementFactory, wampLogScanner) },
            { ScriptStepType.SubScript, new SubScriptStep(Model) },
            { ScriptStepType.Uncheck, new CheckOrUncheckStep(Model, this, outrapHelper, oustScriptStatementFactory, wampLogScanner, ScriptStepType.Uncheck) },
            { ScriptStepType.UncheckSingle, new CheckOrUncheckSingleStep(Model, this, oustScriptStatementFactory, wampLogScanner, ScriptStepType.UncheckSingle) },
            { ScriptStepType.WaitAMinute, new WaitAMinuteStep(Model) },
            { ScriptStepType.WaitTenSeconds, new WaitTenSecondsStep(Model) },
            { ScriptStepType.With, new WithStep(Model, SimpleLogger, this, outrapHelper, oustScriptStatementFactory) },
            { ScriptStepType.WithIdOrClass, new WithIdOrClassStep(Model, SimpleLogger, this, outrapHelper, oustScriptStatementFactory) },
            { ScriptStepType.RecognizeOkay, new RecognizeOkayStep(Model, oustScriptStatementFactory, this) },
            { ScriptStepType.ClearInput, new InputStep(ScriptStepType.ClearInput, Model, SimpleLogger, this, outrapHelper, fileDialogTrickster, oustScriptStatementFactory, oustSettingsHelper) },
            { ScriptStepType.EndScriptIfRecognized, new RecognizeOrNotExpectedContentsStep(Model, SimpleLogger, this, outrapHelper, oustScriptStatementFactory, ScriptStepType.EndScriptIfRecognized) },
            { ScriptStepType.StartOfCleanUpSection, new StartOfCleanUpSectionStep(Model) },
        };

        var selectedValueSelectorHandler = new SelectedValueSelectorHandler(Model, outrapHelper, this);
        var subScriptSelectorHandler = new SubScriptSelectorHandler(Model, this, contextFactory);
        var formOrControlOrIdOrClassHandler = new FormOrControlOrIdOrClassHandler(Model, this, selectedValueSelectorHandler, _ScriptStepLogicDictionary);
        var scriptStepTypeSelectorHandler = new ScriptStepTypeSelectorHandler(Model, this, formOrControlOrIdOrClassHandler, selectedValueSelectorHandler, subScriptSelectorHandler);
        var scriptStepSelectorHandler = new ScriptStepSelectorHandler(Model, this, contextFactory, scriptStepTypeSelectorHandler, formOrControlOrIdOrClassHandler,
                subScriptSelectorHandler, selectedValueSelectorHandler, SimpleLogger);
        var scriptSelectorHandler = new ScriptSelectorHandler(Model, scriptStepSelectorHandler, this, contextFactory);
        Handlers = new ApplicationHandlers {
            ScriptSelectorHandler = scriptSelectorHandler,
            ScriptStepSelectorHandler = scriptStepSelectorHandler,
            ScriptStepTypeSelectorHandler = scriptStepTypeSelectorHandler,
            SelectedValueSelectorHandler = selectedValueSelectorHandler,
            SubScriptSelectorHandler = subScriptSelectorHandler,
            FormOrControlOrIdOrClassHandler = formOrControlOrIdOrClassHandler
        };

        var stepIntoCommand = new StepIntoCommand(Model, Handlers.ScriptSelectorHandler, Handlers.ScriptStepSelectorHandler, this, SimpleLogger,
                _ScriptStepLogicDictionary, outrapHelper, MethodNamesFromStackFramesExtractor);
        var stepOverCommand = new StepOverCommand(Model, stepIntoCommand);
        Commands = new ApplicationCommands {
            AddOrReplaceStepCommand = new AddOrReplaceStepCommand(Model, Handlers.ScriptStepSelectorHandler, this, stepIntoCommand, _ScriptStepLogicDictionary, contextFactory),
            CodeCoverageCommand = new CodeCoverageCommand(this, logicalUrlRepository),
            ConsolidateCommand = new ConsolidateCommand(Model, scriptAndSubScriptsConsolidator, progressWindow),
            DeleteStepCommand = new DeleteStepCommand(Model, Handlers.ScriptStepSelectorHandler, this, contextFactory),
            ExtractSubScriptCommand = new ExtractSubScriptCommand(Model, extractSubScriptPopup, new SubScriptExtractor(contextFactory), Handlers.ScriptSelectorHandler, this),
            MoveUpStepCommand = new MoveUpStepCommand(Model, Handlers.ScriptStepSelectorHandler, this, contextFactory),
            PlayCommand = new PlayCommand(Model, stepOverCommand),
            RecoverCommand = new RecoverCommand(Model, stepOverCommand, Handlers.ScriptStepSelectorHandler),
            RenameCommand = new RenameCommand(Model, Handlers.ScriptSelectorHandler, this, new NewScriptNameValidator(contextFactory), contextFactory),
            DuplicateCommand = new DuplicateCommand(Model, Handlers.ScriptSelectorHandler, this, new NewScriptNameValidator(contextFactory), contextFactory),
            ShowExecutionStackCommand = new ShowExecutionStackCommand(Model, executionStackPopupFactory, executionStackFormatter),
            StepIntoCommand = stepIntoCommand,
            StepOverCommand = stepOverCommand,
            StopCodeCoverageCommand = new StopCodeCoverageCommand(Model, dumperNameConverter, this, logicalUrlRepository),
            SelectScriptFromListCommand = new SelectScriptFromListCommand(Model, selectScriptFromListPopupFactory, scriptSelectorHandler)
        };

        var selectors = new Dictionary<string, ISelector> {
            { nameof(IApplicationModel.SelectedScript), Model.SelectedScript }, { nameof(IApplicationModel.ScriptSteps), Model.ScriptSteps },
            { nameof(IApplicationModel.ScriptStepType), Model.ScriptStepType }, { nameof(IApplicationModel.SelectedValue), Model.SelectedValue },
            { nameof(IApplicationModel.FormOrControlOrIdOrClass), Model.FormOrControlOrIdOrClass }, { nameof(IApplicationModel.SubScript), Model.SubScript }
        };

        var communicator = new TashCommunicator(tashAccessor, this, SimpleLogger, MethodNamesFromStackFramesExtractor);
        var selectorHandler = new TashSelectorHandler(Handlers, obsoleteScriptChecker, SimpleLogger, communicator, selectors, MethodNamesFromStackFramesExtractor);
        var verifyAndSetHandler = new TashVerifyAndSetHandler(SimpleLogger, selectorHandler, communicator, MethodNamesFromStackFramesExtractor, selectors);
        TashHandler = new TashHandler(tashAccessor, SimpleLogger, ButtonNameToCommandMapper, ToggleButtonNameToHandlerMapper, this, verifyAndSetHandler,
            selectorHandler, communicator, MethodNamesFromStackFramesExtractor);
    }

    public override async Task OnLoadedAsync() {
        await base.OnLoadedAsync();
        await Handlers.ScriptSelectorHandler.EnsureNewScriptAsync();
        await Handlers.ScriptSelectorHandler.UpdateSelectableScriptsAsync();
        Handlers.ScriptStepTypeSelectorHandler.UpdateSelectableScriptStepTypes();

        var errorsAndInfos = new ErrorsAndInfos();
        string oustUtilitiesUrl = await logicalUrlRepository.GetUrlAsync("OustUtilitiesJs", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        string jQueryUrl = await logicalUrlRepository.GetUrlAsync("JQuery", errorsAndInfos);
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
        ScriptStepType stepType = Model.ScriptStepType.SelectionMade ? (ScriptStepType)int.Parse(Model.ScriptStepType.SelectedItem.Guid) : ScriptStepType.EndOfScript;
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
        Model.Recover.Enabled = await Commands.RecoverCommand.ShouldBeEnabledAsync();
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