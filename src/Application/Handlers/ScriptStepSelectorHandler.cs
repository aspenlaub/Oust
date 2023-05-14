using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class ScriptStepSelectorHandler : IScriptStepSelectorHandler {
    private readonly IApplicationModel _Model;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;
    private readonly IContextFactory _ContextFactory;
    private readonly IScriptStepTypeSelectorHandler _ScriptStepTypeSelectorHandler;
    private readonly IFormOrControlOrIdOrClassHandler _FormOrControlOrIdOrClassHandler;
    private readonly ISelectedValueSelectorHandler _SelectedValueHandler;
    private readonly ISubScriptSelectorHandler _SubScriptSelectorHandler;
    private readonly ISimpleLogger _SimpleLogger;

    public ScriptStepSelectorHandler(IApplicationModel model, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IContextFactory contextFactory,
            IScriptStepTypeSelectorHandler scriptStepTypeSelectorHandler, IFormOrControlOrIdOrClassHandler formOrControlOrIdOrClassHandler,
            ISubScriptSelectorHandler subScriptSelectorHandler, ISelectedValueSelectorHandler selectedValueHandler, ISimpleLogger simpleLogger) {
        _Model = model;
        _GuiAndAppHandler = guiAndAppHandler;
        _ContextFactory = contextFactory;
        _ScriptStepTypeSelectorHandler = scriptStepTypeSelectorHandler;
        _FormOrControlOrIdOrClassHandler = formOrControlOrIdOrClassHandler;
        _SelectedValueHandler = selectedValueHandler;
        _SubScriptSelectorHandler = subScriptSelectorHandler;
        _SimpleLogger = simpleLogger;
    }

    public async Task ScriptStepsSelectedIndexChangedAsync(int scriptStepsSelectedIndex, bool selectablesChanged) {
        await ScriptStepsSelectedIndexChangedAsync(scriptStepsSelectedIndex, false, selectablesChanged);
    }

    // ReSharper disable once InconsistentNaming
    private async Task ScriptStepsSelectedIndexChangedAsync(int scriptStepsSelectedIndex, bool justAdded, bool selectablesChanged) {
        if (!selectablesChanged && _Model.ScriptSteps.SelectedIndex == scriptStepsSelectedIndex) { return; }

        _Model.ScriptSteps.SelectedIndex = scriptStepsSelectedIndex;
        if (!justAdded) {
            await UpdateStepsDetailsWithCurrentStepAsync();
        }
        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }


    public async Task UpdateSelectableScriptStepsAfterCurrentHasBeenAddedAsync() {
        await UpdateSelectableScriptStepsAsync(CurrentStepRecentAction.Added);
    }

    public async Task UpdateSelectableScriptStepsAfterCurrentHasBeenMovedUpAsync() {
        await UpdateSelectableScriptStepsAsync(CurrentStepRecentAction.MovedUp);
    }

    public async Task UpdateSelectableScriptStepsAsync() {
        await UpdateSelectableScriptStepsAsync(CurrentStepRecentAction.None);
    }

    public async Task UpdateSelectableScriptStepsAfterCurrentHasBeenDeletedAsync() {
        await UpdateSelectableScriptStepsAsync(CurrentStepRecentAction.Deleted);
    }

    private async Task UpdateSelectableScriptStepsAsync(CurrentStepRecentAction currentStepRecentAction) {
        var oldSelectedIndex = _Model.ScriptSteps.SelectedIndex;

        var scriptSteps = new List<ScriptStep>();
        if (_Model.SelectedScript.SelectionMade) {
            await using var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType);
            var script = await context.LoadScriptWithStepsAsync(_Model.SelectedScript.SelectedItem.Guid);
            scriptSteps = script.OrderedScriptSteps();
        }

        var selectables = scriptSteps.Select(s => new Selectable { Guid = s.Guid, Name = s.ToString() }).ToList();
        if (_Model.ScriptSteps.AreSelectablesIdentical(selectables)) { return; }

        _Model.ScriptSteps.UpdateSelectables(selectables);
        if (_Model.ScriptSteps.SelectionMade) {
            switch (currentStepRecentAction) {
                case CurrentStepRecentAction.None:
                case CurrentStepRecentAction.MovedUp:
                    await ScriptStepsSelectedIndexChangedAsync(_Model.ScriptSteps.SelectedIndex, false, true);
                    break;
                case CurrentStepRecentAction.Added:
                    await ScriptStepsSelectedIndexChangedAsync(_Model.ScriptSteps.SelectedIndex + 1, true, true);
                    break;
                case CurrentStepRecentAction.Deleted:
                    await ScriptStepsSelectedIndexChangedAsync(oldSelectedIndex, true, true);
                    break;
                default:
                    throw new NotImplementedException();
            }
        } else {
            await ScriptStepsSelectedIndexChangedAsync(_Model.ScriptSteps.Selectables.Any() ? 0 : -1, false, true);
        }
    }

    private async Task UpdateStepsDetailsWithCurrentStepAsync() {
        if (_Model.ScriptSteps.SelectionMade) {
            using (_SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(UpdateStepsDetailsWithCurrentStepAsync)))) {
                ScriptStep scriptStep;
                await using (var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType)) {
                    scriptStep = await context.ScriptSteps.FirstOrDefaultAsync(s => s.Guid == _Model.ScriptSteps.SelectedItem.Guid);
                }

                if (scriptStep == null) {
                    return;
                }

                await _ScriptStepTypeSelectorHandler.ScriptStepTypeSelectedIndexChangedAsync(_Model.ScriptStepType.Selectables.FindIndex(s => s.Guid == ((int)scriptStep.ScriptStepType).ToString()),
                    false);

                switch (scriptStep.ScriptStepType) {
                    case ScriptStepType.GoToUrl:
                    case ScriptStepType.InvokeUrl:
                        _Model.ScriptStepUrl.Text = scriptStep.Url;
                        break;
                    case ScriptStepType.Recognize:
                    case ScriptStepType.NotExpectedContents:
                    case ScriptStepType.NotExpectedSelection:
                    case ScriptStepType.EndScriptIfRecognized:
                        _Model.ExpectedContents.Text = scriptStep.ExpectedContents;
                        break;
                    case ScriptStepType.Input:
                    case ScriptStepType.InputIntoSingle:
                        _Model.ScriptStepInput.Text = scriptStep.InputText;
                        break;
                    default:
                        _Model.FreeText.Text = "";
                        break;
                }

                int selectedIndex;
                switch (scriptStep.ScriptStepType) {
                    case ScriptStepType.With:
                        selectedIndex = _Model.FormOrControlOrIdOrClass.Selectables.FindIndex(s => s.Guid == scriptStep.FormGuid);
                        await _FormOrControlOrIdOrClassHandler.FormOrControlOrIdOrClassSelectedIndexChangedAsync(selectedIndex, false);
                        await _FormOrControlOrIdOrClassHandler.FormOrIdOrClassInstanceNumberChangedAsync(scriptStep.FormInstanceNumber.ToString());
                        _Model.ScriptStepOutrapForm = new Selectable { Guid = scriptStep.FormGuid, Name = scriptStep.FormName };
                        break;
                    case ScriptStepType.Recognize:
                    case ScriptStepType.NotExpectedContents:
                    case ScriptStepType.Check:
                    case ScriptStepType.Uncheck:
                    case ScriptStepType.Press:
                    case ScriptStepType.Input:
                    case ScriptStepType.ClearInput:
                    case ScriptStepType.Select:
                    case ScriptStepType.NotExpectedSelection:
                    case ScriptStepType.RecognizeSelection:
                    case ScriptStepType.EndScriptIfRecognized:
                        selectedIndex = _Model.FormOrControlOrIdOrClass.Selectables.FindIndex(s => s.Guid == scriptStep.ControlGuid);
                        await _FormOrControlOrIdOrClassHandler.FormOrControlOrIdOrClassSelectedIndexChangedAsync(selectedIndex, false);
                        _Model.ScriptStepOutOfControl = new Selectable { Guid = scriptStep.ControlGuid, Name = scriptStep.ControlName };
                        break;
                    case ScriptStepType.WithIdOrClass:
                    case ScriptStepType.NotExpectedIdOrClass:
                        selectedIndex = _Model.FormOrControlOrIdOrClass.Selectables.FindIndex(s => s.Guid == scriptStep.IdOrClass);
                        await _FormOrControlOrIdOrClassHandler.FormOrControlOrIdOrClassSelectedIndexChangedAsync(selectedIndex, false);
                        await _FormOrControlOrIdOrClassHandler.FormOrIdOrClassInstanceNumberChangedAsync(scriptStep.IdOrClassInstanceNumber.ToString());
                        _Model.ScriptStepIdOrClass = scriptStep.IdOrClass;
                        break;
                    case ScriptStepType.GoToUrl:
                    case ScriptStepType.CheckSingle:
                    case ScriptStepType.UncheckSingle:
                    case ScriptStepType.PressSingle:
                    case ScriptStepType.InputIntoSingle:
                    case ScriptStepType.SubScript:
                    case ScriptStepType.WaitAMinute:
                    case ScriptStepType.EndOfScript:
                    case ScriptStepType.WaitTenSeconds:
                    case ScriptStepType.InvokeUrl:
                    case ScriptStepType.RecognizeOkay:
                        await _FormOrControlOrIdOrClassHandler.FormOrControlOrIdOrClassSelectedIndexChangedAsync(-1, false);
                        await _FormOrControlOrIdOrClassHandler.FormOrIdOrClassInstanceNumberChangedAsync("");
                        _Model.ScriptStepOutrapForm = null;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                switch (scriptStep.ScriptStepType) {
                    case ScriptStepType.Select:
                        selectedIndex = _Model.SelectedValue.Selectables.FindIndex(s => s.Name == scriptStep.InputText);
                        await _SelectedValueHandler.SelectedValueSelectedIndexChangedAsync(selectedIndex, false);
                    break;
                    case ScriptStepType.RecognizeSelection:
                        selectedIndex = _Model.SelectedValue.Selectables.FindIndex(s => s.Name == scriptStep.ExpectedContents);
                        await _SelectedValueHandler.SelectedValueSelectedIndexChangedAsync(selectedIndex, false);
                    break;
                    default:
                        await _SelectedValueHandler.SelectedValueSelectedIndexChangedAsync(-1, false);
                    break;
                }

                if (scriptStep.ScriptStepType == ScriptStepType.SubScript) {
                    selectedIndex = _Model.SubScript.Selectables.FindIndex(s => s.Guid == scriptStep.SubScriptGuid);
                    await _SubScriptSelectorHandler.SubScriptSelectedIndexChangedAsync(selectedIndex, false);
                }
                else {
                    await _SubScriptSelectorHandler.SubScriptSelectedIndexChangedAsync(-1, false);
                }
            }
        } else {
            await _ScriptStepTypeSelectorHandler.ScriptStepTypeSelectedIndexChangedAsync(-1, true);
            _Model.ScriptStepOutrapForm = null;
        }

        _Model.Status.Text = "";
        _Model.Status.Type = StatusType.None;
        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }
}