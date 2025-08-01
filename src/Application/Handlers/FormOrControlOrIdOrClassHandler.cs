using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class FormOrControlOrIdOrClassHandler(IApplicationModel model, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler,
        ISelectedValueSelectorHandler selectedValueSelectorHandler, IDictionary<ScriptStepType, IScriptStepLogic> scriptStepLogicDictionary)
            : IFormOrControlOrIdOrClassHandler {
    public async Task EnableOrDisableFormOrControlOrIdOrClassAndSetLabelTextAsync() {
        ScriptStepType scriptStepType = model.ScriptStepType.SelectionMade ? (ScriptStepType)int.Parse(model.ScriptStepType.SelectedItem.Guid) : ScriptStepType.EndOfScript;
        scriptStepLogicDictionary[scriptStepType].SetFormOrControlOrIdOrClassTitle();
        await UpdateSelectableFormsOrControlsOrIdsOrClassesAsync();
    }

    private async Task UpdateSelectableFormsOrControlsOrIdsOrClassesAsync() {
        guiAndAppHandler.IndicateBusy(true);
        ScriptStepType scriptStepType = model.ScriptStepType.SelectionMade ? (ScriptStepType)int.Parse(model.ScriptStepType.SelectedItem.Guid) : ScriptStepType.EndOfScript;
        IList<Selectable> selectables = await scriptStepLogicDictionary[scriptStepType].SelectableFormsOrControlsOrIdsOrClassesAsync();
        if (model.FormOrControlOrIdOrClass.AreSelectablesIdentical(selectables)) { return; }

        model.FormOrControlOrIdOrClass.UpdateSelectables(selectables);
        if (model.FormOrControlOrIdOrClass.SelectionMade) {
            await FormOrControlOrIdOrClassSelectedIndexChangedAsync(model.FormOrControlOrIdOrClass.SelectedIndex, true);
        } else {
            await FormOrControlOrIdOrClassSelectedIndexChangedAsync(model.FormOrControlOrIdOrClass.Selectables.Any() ? 0 : -1, true);
        }
    }

    public async Task FormOrIdOrClassInstanceNumberChangedAsync(string text) {
        if (model.FormOrIdOrClassInstanceNumber.Text == text) { return; }

        model.FormOrIdOrClassInstanceNumber.Text = text;
        await guiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }

    public async Task FormOrControlOrIdOrClassSelectedIndexChangedAsync(int selectedIndex, bool selectablesChanged) {
        if (!selectablesChanged && model.FormOrControlOrIdOrClass.SelectedIndex == selectedIndex) { return; }

        model.FormOrControlOrIdOrClass.SelectedIndex = selectedIndex;
        ScriptStepType scriptStepType = model.ScriptStepType.SelectionMade && model.ScriptStepType.SelectedItem != null ? (ScriptStepType)int.Parse(model.ScriptStepType.SelectedItem.Guid) : ScriptStepType.EndOfScript;
        model.ScriptStepOutrapForm = null;
        model.ScriptStepOutOfControl = null;
        model.ScriptStepIdOrClass = "";
        switch (scriptStepType) {
            case ScriptStepType.With:
                model.ScriptStepOutrapForm = model.FormOrControlOrIdOrClass.SelectedItem;
                break;
            case ScriptStepType.Recognize:
            case ScriptStepType.NotExpectedContents:
            case ScriptStepType.Check:
            case ScriptStepType.Uncheck:
            case ScriptStepType.Press:
            case ScriptStepType.Input:
            case ScriptStepType.ClearInput:
            case ScriptStepType.Select:
            case ScriptStepType.RecognizeSelection:
            case ScriptStepType.NotExpectedSelection:
            case ScriptStepType.EndScriptIfRecognized:
                model.ScriptStepOutOfControl = model.FormOrControlOrIdOrClass.SelectedItem;
                break;
            case ScriptStepType.WithIdOrClass:
            case ScriptStepType.NotExpectedIdOrClass:
                model.ScriptStepIdOrClass = model.FormOrControlOrIdOrClass.SelectionMade ? model.FormOrControlOrIdOrClass.SelectedItem.Guid : "";
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
                break;
            default:
                throw new NotImplementedException();
        }

        bool anySelectables = model.FormOrControlOrIdOrClass.Selectables.Any();
        model.FormOrIdOrClassInstanceNumber.Enabled = anySelectables && model.FormOrControlOrIdOrClass.SelectedIndex > 0 && scriptStepType != ScriptStepType.Select;
        if (!model.FormOrIdOrClassInstanceNumber.Enabled) {
            await FormOrIdOrClassInstanceNumberChangedAsync("");
        } else if (string.IsNullOrWhiteSpace(model.FormOrIdOrClassInstanceNumber.Text)) {
            await FormOrIdOrClassInstanceNumberChangedAsync("1");
        }

        await selectedValueSelectorHandler.EnableOrDisableSelectedValueAsync();

        await guiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }
}