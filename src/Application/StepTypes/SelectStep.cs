using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class SelectStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler,
        IOutrapHelper outrapHelper, IOustScriptStatementFactory oustScriptStatementFactory, IWampLogScanner wampLogScanner)
            : IScriptStepLogic {
    public IApplicationModel Model { get; init; } = model;
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; } = guiAndAppHandler;
    public ISimpleLogger SimpleLogger { get; init; } = simpleLogger;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!await ShouldBeEnabledAsync()) { return false; }

        return !string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid) && Model.SelectedValue.SelectionMade && Model.SelectedValue.SelectedIndex > 0;
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep {
            ScriptStepType = ScriptStepType.Select,
            ControlGuid = Model.ScriptStepOutOfControl.Guid,
            ControlName = Model.ScriptStepOutOfControl.Name,
            InputText = Model.SelectedValue.SelectedItem.Name
        };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        IScriptStatement scriptStatement = oustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.WithScriptStepOutrapForm.Guid, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name);
        ScriptCallResponse scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        string ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = oustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, Model.ScriptStepOutOfControl.Guid, Model.ScriptStepOutOfControl.Name, Model.WithScriptStepOutrapForm.Name, Model.WithScriptStepOutrapFormInstanceNumber);
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        if (!Model.SelectedValue.SelectionMade) {
            Model.Status.Text = string.Format(Properties.Resources.OptionToSelectNotFound, Model.ScriptStepOutOfControl?.Name ?? "?");
            Model.Status.Type = StatusType.Error;
            return;
        }

        DateTime startOfExecutionTimeStamp = wampLogScanner.WaitUntilLogFolderIsErrorFreeReturnStartOfExecutionTimeStamp();

        string domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = new ScriptStatement {
            Statement = "OustActions.SelectAnOption(" + domElementJson + ", \"" + Model.SelectedValue.SelectedItem.Guid + "\")",
            NoSuccessErrorMessage = string.Format(Properties.Resources.OptionToSelectNotFound, Model.ScriptStepOutOfControl?.Name ?? "?")
        };
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        wampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out string errorMessage);
        if (!string.IsNullOrEmpty(errorMessage)) {
            Model.Status.Text = errorMessage;
            Model.Status.Type = StatusType.Error;
            return;
        }

        Model.Status.Text = "";
        Model.Status.Type = StatusType.None;
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.ControlTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await outrapHelper.OutOfControlChoicesAsync(ScriptStepType.Select, Model.WithScriptStepOutrapForm?.Guid, Model.WithScriptStepOutrapFormInstanceNumber);
    }
}