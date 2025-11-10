using System.Text.Json;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class CheckOrUncheckStep(IApplicationModel model,
        IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler,
        IOutrapHelper outrapHelper, IOustScriptStatementFactory oustScriptStatementFactory,
        IWampLogScanner wampLogScanner, ScriptStepType stepType)
            : IScriptStepLogic {
    public IApplicationModel Model { get; init; } = model;
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; } = guiAndAppHandler;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!await ShouldBeEnabledAsync()) { return false; }

        return !string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid);
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep { ScriptStepType = stepType,
            ControlGuid = Model.ScriptStepOutOfControl.Guid,
            ControlName = Model.ScriptStepOutOfControl.Name
        };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        if (stepType != ScriptStepType.Check && stepType != ScriptStepType.Uncheck) { return false; }
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        DateTime startOfExecutionTimeStamp = DateTime.Now;

        IScriptStatement scriptStatement = oustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.WithScriptStepOutrapForm.Guid, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name);
        ScriptCallResponse scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        string ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = oustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, Model.ScriptStepOutOfControl.Guid, Model.ScriptStepOutOfControl.Name, "", 0);
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        string domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = oustScriptStatementFactory.CreateCheckOrUncheckStatement(domElementJson, stepType == ScriptStepType.Check);
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        wampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out string errorMessage);

        Model.Status.Text = errorMessage;
        Model.Status.Type = string.IsNullOrWhiteSpace(errorMessage) ? StatusType.None : StatusType.Error;
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.ControlTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await outrapHelper.OutOfControlChoicesAsync(stepType, Model.WithScriptStepOutrapForm?.Guid, Model.WithScriptStepOutrapFormInstanceNumber);
    }
}