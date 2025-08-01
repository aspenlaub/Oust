using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class ScriptStepSelectorHandler(IApplicationModel model, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler,
        IContextFactory contextFactory, IScriptStepTypeSelectorHandler scriptStepTypeSelectorHandler,
        IFormOrControlOrIdOrClassHandler formOrControlOrIdOrClassHandler, ISubScriptSelectorHandler subScriptSelectorHandler,
        ISelectedValueSelectorHandler selectedValueHandler, ISimpleLogger simpleLogger) : IScriptStepSelectorHandler {
    public async Task ScriptStepsSelectedIndexChangedAsync(int scriptStepsSelectedIndex, bool selectablesChanged) {
        await ScriptStepsSelectedIndexChangedAsync(scriptStepsSelectedIndex, false, selectablesChanged);
    }

    // ReSharper disable once InconsistentNaming
    private async Task ScriptStepsSelectedIndexChangedAsync(int scriptStepsSelectedIndex, bool justAdded, bool selectablesChanged) {
        if (!selectablesChanged && model.ScriptSteps.SelectedIndex == scriptStepsSelectedIndex) { return; }

        model.ScriptSteps.SelectedIndex = scriptStepsSelectedIndex;
        if (!justAdded) {
            await UpdateStepsDetailsWithCurrentStepAsync();
        }
        await guiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
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
        int oldSelectedIndex = model.ScriptSteps.SelectedIndex;

        var scriptSteps = new List<ScriptStep>();
        if (model.SelectedScript.SelectionMade) {
            await using Context context = await contextFactory.CreateAsync(model.EnvironmentType);
            Script script = await context.LoadScriptWithStepsAsync(model.SelectedScript.SelectedItem.Guid);
            scriptSteps = script.OrderedScriptSteps();
        }

        var selectables = scriptSteps.Select(s => new Selectable { Guid = s.Guid, Name = s.ToString() }).ToList();
        if (model.ScriptSteps.AreSelectablesIdentical(selectables)) { return; }

        model.ScriptSteps.UpdateSelectables(selectables);
        if (model.ScriptSteps.SelectionMade) {
            switch (currentStepRecentAction) {
                case CurrentStepRecentAction.None:
                case CurrentStepRecentAction.MovedUp:
                    await ScriptStepsSelectedIndexChangedAsync(model.ScriptSteps.SelectedIndex, false, true);
                    break;
                case CurrentStepRecentAction.Added:
                    await ScriptStepsSelectedIndexChangedAsync(model.ScriptSteps.SelectedIndex + 1, true, true);
                    break;
                case CurrentStepRecentAction.Deleted:
                    await ScriptStepsSelectedIndexChangedAsync(oldSelectedIndex, true, true);
                    break;
                default:
                    throw new NotImplementedException();
            }
        } else {
            await ScriptStepsSelectedIndexChangedAsync(model.ScriptSteps.Selectables.Any() ? 0 : -1, false, true);
        }
    }

    private async Task UpdateStepsDetailsWithCurrentStepAsync() {
        if (model.ScriptSteps.SelectionMade) {
            using (simpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(UpdateStepsDetailsWithCurrentStepAsync)))) {
                ScriptStep scriptStep;
                await using (Context context = await contextFactory.CreateAsync(model.EnvironmentType)) {
                    scriptStep = await context.ScriptSteps.FirstOrDefaultAsync(s => s.Guid == model.ScriptSteps.SelectedItem.Guid);
                }

                if (scriptStep == null) {
                    return;
                }

                await scriptStepTypeSelectorHandler.ScriptStepTypeSelectedIndexChangedAsync(model.ScriptStepType.Selectables.FindIndex(s => s.Guid == ((int)scriptStep.ScriptStepType).ToString()),
                    false);

                switch (scriptStep.ScriptStepType) {
                    case ScriptStepType.GoToUrl:
                    case ScriptStepType.InvokeUrl:
                        model.ScriptStepUrl.Text = scriptStep.Url;
                        break;
                    case ScriptStepType.Recognize:
                    case ScriptStepType.NotExpectedContents:
                    case ScriptStepType.NotExpectedSelection:
                    case ScriptStepType.EndScriptIfRecognized:
                        model.ExpectedContents.Text = scriptStep.ExpectedContents;
                        break;
                    case ScriptStepType.Input:
                    case ScriptStepType.InputIntoSingle:
                        model.ScriptStepInput.Text = scriptStep.InputText;
                        break;
                    default:
                        model.FreeText.Text = "";
                        break;
                }

                int selectedIndex;
                switch (scriptStep.ScriptStepType) {
                    case ScriptStepType.With:
                        selectedIndex = model.FormOrControlOrIdOrClass.Selectables.FindIndex(s => s.Guid == scriptStep.FormGuid);
                        await formOrControlOrIdOrClassHandler.FormOrControlOrIdOrClassSelectedIndexChangedAsync(selectedIndex, false);
                        await formOrControlOrIdOrClassHandler.FormOrIdOrClassInstanceNumberChangedAsync(scriptStep.FormInstanceNumber.ToString());
                        model.ScriptStepOutrapForm = new Selectable { Guid = scriptStep.FormGuid, Name = scriptStep.FormName };
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
                        selectedIndex = model.FormOrControlOrIdOrClass.Selectables.FindIndex(s => s.Guid == scriptStep.ControlGuid);
                        await formOrControlOrIdOrClassHandler.FormOrControlOrIdOrClassSelectedIndexChangedAsync(selectedIndex, false);
                        model.ScriptStepOutOfControl = new Selectable { Guid = scriptStep.ControlGuid, Name = scriptStep.ControlName };
                        break;
                    case ScriptStepType.WithIdOrClass:
                    case ScriptStepType.NotExpectedIdOrClass:
                        selectedIndex = model.FormOrControlOrIdOrClass.Selectables.FindIndex(s => s.Guid == scriptStep.IdOrClass);
                        await formOrControlOrIdOrClassHandler.FormOrControlOrIdOrClassSelectedIndexChangedAsync(selectedIndex, false);
                        await formOrControlOrIdOrClassHandler.FormOrIdOrClassInstanceNumberChangedAsync(scriptStep.IdOrClassInstanceNumber.ToString());
                        model.ScriptStepIdOrClass = scriptStep.IdOrClass;
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
                    case ScriptStepType.StartOfCleanUpSection:
                        await formOrControlOrIdOrClassHandler.FormOrControlOrIdOrClassSelectedIndexChangedAsync(-1, false);
                        await formOrControlOrIdOrClassHandler.FormOrIdOrClassInstanceNumberChangedAsync("");
                        model.ScriptStepOutrapForm = null;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                switch (scriptStep.ScriptStepType) {
                    case ScriptStepType.Select:
                        selectedIndex = model.SelectedValue.Selectables.FindIndex(s => s.Name == scriptStep.InputText);
                        await selectedValueHandler.SelectedValueSelectedIndexChangedAsync(selectedIndex, false);
                    break;
                    case ScriptStepType.RecognizeSelection:
                        selectedIndex = model.SelectedValue.Selectables.FindIndex(s => s.Name == scriptStep.ExpectedContents);
                        await selectedValueHandler.SelectedValueSelectedIndexChangedAsync(selectedIndex, false);
                    break;
                    default:
                        await selectedValueHandler.SelectedValueSelectedIndexChangedAsync(-1, false);
                    break;
                }

                if (scriptStep.ScriptStepType == ScriptStepType.SubScript) {
                    selectedIndex = model.SubScript.Selectables.FindIndex(s => s.Guid == scriptStep.SubScriptGuid);
                    await subScriptSelectorHandler.SubScriptSelectedIndexChangedAsync(selectedIndex, false);
                }
                else {
                    await subScriptSelectorHandler.SubScriptSelectedIndexChangedAsync(-1, false);
                }
            }
        } else {
            await scriptStepTypeSelectorHandler.ScriptStepTypeSelectedIndexChangedAsync(-1, true);
            model.ScriptStepOutrapForm = null;
        }

        model.Status.Text = "";
        model.Status.Type = StatusType.None;
        await guiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }
}