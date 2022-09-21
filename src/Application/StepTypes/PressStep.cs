using System.Text.Json;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class PressStep : IScriptStepLogic {
    public IApplicationModel Model { get; init; }
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; }
    public ISimpleLogger SimpleLogger { get; init; }
    private readonly IOutrapHelper _OutrapHelper;
    private readonly IWampLogScanner _WampLogScanner;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public PressStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IOutrapHelper outrapHelper, IWampLogScanner wampLogScanner, IOustScriptStatementFactory oustScriptStatementFactory) {
        Model = model;
        GuiAndAppHandler = guiAndAppHandler;
        SimpleLogger = simpleLogger;
        _OutrapHelper = outrapHelper;
        _WampLogScanner = wampLogScanner;
        _OustScriptStatementFactory = oustScriptStatementFactory;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!await ShouldBeEnabledAsync()) { return false; }

        return !string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid);
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep {
            ScriptStepType = ScriptStepType.Press,
            ControlGuid = Model.ScriptStepOutOfControl.Guid,
            ControlName = Model.ScriptStepOutOfControl.Name
        };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(PressStep) + nameof(IScriptStepLogic.ExecuteAsync)))) {
            var startOfExecutionTimeStamp = DateTime.Now;

            var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.WithScriptStepOutrapForm.Guid, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name);
            var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            var ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);

            scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, Model.ScriptStepOutOfControl.Guid, Model.ScriptStepOutOfControl.Name, Model.WithScriptStepOutrapForm.Name, Model.WithScriptStepOutrapFormInstanceNumber);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
                return;
            }

            var domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
            scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfIdStatement(domElementJson, "selectuploadfile", "");
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
            var isUpload = scriptCallResponse.Success.YesNo && !scriptCallResponse.Success.Inconclusive;

            scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainAnchorOrSubmitStatement(domElementJson, isUpload ? 2 : 1);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
            scriptStatement = _OustScriptStatementFactory.CreateClickAnchorStatement(domElementJson);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            await GuiAndAppHandler.WaitUntilNotNavigatingAnymoreAsync();

            _WampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out var errorMessage);

            await GuiAndAppHandler.WaitUntilNotNavigatingAnymoreAsync();

            Model.Status.Text = errorMessage;
            Model.Status.Type = string.IsNullOrWhiteSpace(errorMessage) ? StatusType.None : StatusType.Error;
        }
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.ControlTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await _OutrapHelper.OutOfControlChoicesAsync(ScriptStepType.Press, Model.WithScriptStepOutrapForm?.Guid, Model.WithScriptStepOutrapFormInstanceNumber);
    }
}