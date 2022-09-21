using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class NotExpectedIdOrClassStep : IScriptStepLogic {
    public IApplicationModel Model { get; init; }
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; }
    public ISimpleLogger SimpleLogger { get; init; }
    private readonly IOutrapHelper _OutrapHelper;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public NotExpectedIdOrClassStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IOutrapHelper outrapHelper, IOustScriptStatementFactory oustScriptStatementFactory) {
        Model = model;
        GuiAndAppHandler = guiAndAppHandler;
        SimpleLogger = simpleLogger;
        _OutrapHelper = outrapHelper;
        _OustScriptStatementFactory = oustScriptStatementFactory;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!ShouldBeEnabled(out var instanceNumber)) { return false; }
        if (!Model.FormOrControlOrIdOrClass.SelectionMade || Model.FormOrControlOrIdOrClass.SelectedIndex <= 0) { return false; }

        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(Model.FormOrControlOrIdOrClass.SelectedItem.Guid, instanceNumber);
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        return instanceNumber > 0 && !scriptCallResponse.Success.YesNo && !scriptCallResponse.Success.Inconclusive;
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep {
            ScriptStepType = ScriptStepType.NotExpectedIdOrClass,
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

    public async Task ExecuteAsync() {
        var instanceNumber = int.Parse(Model.FormOrIdOrClassInstanceNumber.Text);
        var idOrClass = Model.FormOrControlOrIdOrClass.SelectedItem?.Guid ?? Model.ScriptStepIdOrClass;
        if (string.IsNullOrWhiteSpace(idOrClass)) {
            Model.Status.Text = string.Format(Properties.Resources.UnexpectedInstanceXOfYFound, "(?)", instanceNumber);
            Model.Status.Type = StatusType.Error;
        } else {
            var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(idOrClass, instanceNumber);
            var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, false);
            if (scriptCallResponse.Success.Inconclusive || scriptCallResponse.Success.YesNo) {
                return;
            }

            Model.Status.Text = "";
            Model.Status.Type = StatusType.None;
        }
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.IdOrClassTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await _OutrapHelper.IdOrClassChoicesAsync();
    }
}