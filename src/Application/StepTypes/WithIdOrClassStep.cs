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

public class WithIdOrClassStep : IScriptStepLogic {
    public IApplicationModel Model { get; init; }
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; }
    public ISimpleLogger SimpleLogger { get; init; }
    private readonly IOucoHelper _OucoHelper;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public WithIdOrClassStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IOucoHelper oucoHelper, IOustScriptStatementFactory oustScriptStatementFactory) {
        Model = model;
        SimpleLogger = simpleLogger;
        GuiAndAppHandler = guiAndAppHandler;
        _OucoHelper = oucoHelper;
        _OustScriptStatementFactory = oustScriptStatementFactory;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!ShouldBeEnabled(out var instanceNumber)) { return false; }
        if (!Model.FormOrControlOrIdOrClass.SelectionMade || Model.FormOrControlOrIdOrClass.SelectedIndex <= 0) { return false; }

        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(Model.FormOrControlOrIdOrClass.SelectedItem.Guid, instanceNumber);
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        return scriptCallResponse.Success.YesNo && !scriptCallResponse.Success.Inconclusive;

    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep {
            ScriptStepType = ScriptStepType.WithIdOrClass,
            IdOrClass = Model.FormOrControlOrIdOrClass.SelectedItem.Guid,
            IdOrClassInstanceNumber = int.Parse(Model.FormOrIdOrClassInstanceNumber.Text)
        };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(ShouldBeEnabled(out _));
    }

    private bool ShouldBeEnabled(out int instanceNumber) {
        instanceNumber = -1;
        if (string.IsNullOrWhiteSpace(Model.ScriptStepIdOrClass)) { return false; }

        int.TryParse(Model.FormOrIdOrClassInstanceNumber.Text, out instanceNumber);
        return instanceNumber > 0;
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.IdOrClassTitle;
    }

    public async Task ExecuteAsync() {
        var instanceNumber = int.Parse(Model.FormOrIdOrClassInstanceNumber.Text);
        if (!Model.FormOrControlOrIdOrClass.SelectionMade) {
            Model.Status.Text = string.Format(Properties.Resources.InstanceXOfYNotFound, instanceNumber,
                string.IsNullOrWhiteSpace(Model.ScriptStepIdOrClass) ? "(?)" : Model.ScriptStepIdOrClass);
            Model.Status.Type = StatusType.Error;
            return;
        }

        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(Model.FormOrControlOrIdOrClass.SelectedItem.Guid, instanceNumber);
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        Model.WithScriptStepOucoOrOutrapForm = null;
        Model.WithScriptStepOucoOrOutrapFormInstanceNumber = 0;
        Model.WithScriptStepIdOrClass = Model.FormOrControlOrIdOrClass.SelectedItem.Guid;
        Model.WithScriptStepIdOrClassInstanceNumber = instanceNumber;

        var elementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = new ScriptStatement { Statement = "OustActions.AlterVisualAppearance(" + elementJson + ")" };
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        Model.Status.Text = "";
        Model.Status.Type = StatusType.None;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await _OucoHelper.IdOrClassChoicesAsync();
    }
}