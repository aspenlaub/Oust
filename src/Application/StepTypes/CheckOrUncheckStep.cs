using System.Text.Json;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class CheckOrUncheckStep : IScriptStepLogic {
    public IApplicationModel Model { get; init; }
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; }
    public ISimpleLogger SimpleLogger { get; init; }
    private readonly IOutrapHelper _OutrapHelper;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;

    private readonly ScriptStepType _ScriptStepType;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public CheckOrUncheckStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IOutrapHelper outrapHelper, IOustScriptStatementFactory oustScriptStatementFactory, ScriptStepType stepType) {
        Model = model;
        GuiAndAppHandler = guiAndAppHandler;
        SimpleLogger = simpleLogger;
        _OutrapHelper = outrapHelper;
        _ScriptStepType = stepType;
        _OustScriptStatementFactory = oustScriptStatementFactory;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!await ShouldBeEnabledAsync()) { return false; }

        return !string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid);
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep { ScriptStepType = _ScriptStepType,
            ControlGuid = Model.ScriptStepOutOfControl.Guid,
            ControlName = Model.ScriptStepOutOfControl.Name
        };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        if (_ScriptStepType != ScriptStepType.Check && _ScriptStepType != ScriptStepType.Uncheck) { return false; }
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.WithScriptStepOutrapForm.Guid, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name);
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        var ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, Model.ScriptStepOutOfControl.Guid, Model.ScriptStepOutOfControl.Name, "", 0);
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        var domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = _OustScriptStatementFactory.CreateCheckOrUncheckStatement(domElementJson, _ScriptStepType == ScriptStepType.Check);
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        Model.Status.Text = "";
        Model.Status.Type = StatusType.None;
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.ControlTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await _OutrapHelper.OutOfControlChoicesAsync(_ScriptStepType, Model.WithScriptStepOutrapForm?.Guid, Model.WithScriptStepOutrapFormInstanceNumber);
    }
}