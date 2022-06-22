using System.Text.Json;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class RecognizeOkayStep : IScriptStepLogic {
    private readonly IApplicationModel _Model;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;
    private IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;

    public RecognizeOkayStep(IApplicationModel model, IOustScriptStatementFactory oustScriptStatementFactory, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler) {
        _Model = model;
        _OustScriptStatementFactory = oustScriptStatementFactory;
        _GuiAndAppHandler = guiAndAppHandler;
    }

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        return Task.FromResult(true);
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep { ScriptStepType = ScriptStepType.RecognizeOkay };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement("response", 1);
        var scriptCallResponse = await _GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        var domElementInnerHtml = scriptCallResponse.InnerHtml;
        if (domElementInnerHtml.Contains("OK")) { return; }

        _Model.Status.Type = StatusType.Error;
        _Model.Status.Text = Properties.Resources.ResponseIsNotOkay;
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        _Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.FormOrControlOrIdOrClassTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await Task.FromResult(new List<Selectable>());
    }
}