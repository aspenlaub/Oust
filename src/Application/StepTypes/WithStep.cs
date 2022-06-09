using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class WithStep : IScriptStepLogic {
    public IApplicationModel Model { get; init; }
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; }
    public ISimpleLogger SimpleLogger { get; init; }
    private readonly IOucoHelper _OucoHelper;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public WithStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IOucoHelper oucoHelper, IOustScriptStatementFactory oustScriptStatementFactory) {
        Model = model;
        SimpleLogger = simpleLogger;
        GuiAndAppHandler = guiAndAppHandler;
        _OucoHelper = oucoHelper;
        _OustScriptStatementFactory = oustScriptStatementFactory;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!ShouldBeEnabled(out var instanceNumber)) { return false; }
        if (!Model.FormOrControlOrIdOrClass.SelectionMade) { return false; }

        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.ScriptStepOucoOrOutrapForm.Guid, instanceNumber, Model.ScriptStepOucoOrOutrapForm.Name);
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        return scriptCallResponse.Success.YesNo && !scriptCallResponse.Success.Inconclusive;
    }

    public IScriptStep CreateScriptStepToAdd() {
        var instanceNumber = int.Parse(Model.FormOrIdOrClassInstanceNumber.Text);
        return new ScriptStep { ScriptStepType = ScriptStepType.With, FormGuid = Model.FormOrControlOrIdOrClass.SelectedItem.Guid, FormName = Model.FormOrControlOrIdOrClass.SelectedItem.Name, FormInstanceNumber = instanceNumber };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(ShouldBeEnabled(out _));
    }

    private bool ShouldBeEnabled(out int instanceNumber) {
        instanceNumber = -1;
        if (string.IsNullOrWhiteSpace(Model.ScriptStepOucoOrOutrapForm?.Guid)) { return false; }

        int.TryParse(Model.FormOrIdOrClassInstanceNumber.Text, out instanceNumber);
        return instanceNumber > 0;
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.FormTitle;
    }

    public async Task ExecuteAsync() {
        var instanceNumber = int.Parse(Model.FormOrIdOrClassInstanceNumber.Text);
        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.ScriptStepOucoOrOutrapForm.Guid, instanceNumber, Model.ScriptStepOucoOrOutrapForm.Name);
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        Model.WithScriptStepOucoOrOutrapForm = new Selectable { Guid = Model.ScriptStepOucoOrOutrapForm.Guid, Name = Model.ScriptStepOucoOrOutrapForm.Name };
        Model.WithScriptStepOucoOrOutrapFormInstanceNumber = instanceNumber;

        Model.Status.Text = "";
        Model.Status.Type = StatusType.None;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await _OucoHelper.FormChoicesAsync();
    }
}