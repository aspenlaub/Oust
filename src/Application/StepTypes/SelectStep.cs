using System.Text.Json;
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

public class SelectStep : IScriptStepLogic {
    public IApplicationModel Model { get; init; }
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; }
    public ISimpleLogger SimpleLogger { get; init; }
    private readonly IOucoHelper _OucoHelper;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;
    private readonly IWampLogScanner _WampLogScanner;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public SelectStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IOucoHelper oucoHelper,
                      IOustScriptStatementFactory oustScriptStatementFactory, IWampLogScanner wampLogScanner) {
        Model = model;
        SimpleLogger = simpleLogger;
        GuiAndAppHandler = guiAndAppHandler;
        _OucoHelper = oucoHelper;
        _OustScriptStatementFactory = oustScriptStatementFactory;
        _WampLogScanner = wampLogScanner;
    }

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
        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.WithScriptStepOucoOrOutrapForm.Guid, Model.WithScriptStepOucoOrOutrapFormInstanceNumber, Model.WithScriptStepOucoOrOutrapForm.Name);
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        var ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, Model.ScriptStepOutOfControl.Guid, Model.ScriptStepOutOfControl.Name, Model.WithScriptStepOucoOrOutrapForm.Name, Model.WithScriptStepOucoOrOutrapFormInstanceNumber);
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        if (!Model.SelectedValue.SelectionMade) {
            Model.Status.Text = string.Format(Properties.Resources.OptionToSelectNotFound, Model.ScriptStepOutOfControl?.Name ?? "?");
            Model.Status.Type = StatusType.Error;
            return;
        }

        var startOfExecutionTimeStamp = DateTime.Now;

        var domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = new ScriptStatement {
            Statement = "OustActions.SelectAnOption(" + domElementJson + ", \"" + Model.SelectedValue.SelectedItem.Guid + "\")",
            NoSuccessErrorMessage = string.Format(Properties.Resources.OptionToSelectNotFound, Model.ScriptStepOutOfControl?.Name ?? "?")
        };
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        _WampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out var errorMessage);
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
        return await _OucoHelper.OutOfControlChoicesAsync(ScriptStepType.Select, Model.WithScriptStepOucoOrOutrapForm?.Guid, Model.WithScriptStepOucoOrOutrapFormInstanceNumber);
    }
}