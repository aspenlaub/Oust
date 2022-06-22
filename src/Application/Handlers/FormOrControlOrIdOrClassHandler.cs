using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class FormOrControlOrIdOrClassHandler : IFormOrControlOrIdOrClassHandler {
    private readonly IApplicationModel _Model;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;
    private readonly ISelectedValueSelectorHandler _SelectedValueSelectorHandler;
    private readonly IDictionary<ScriptStepType, IScriptStepLogic> _ScriptStepLogicDictionary;

    public FormOrControlOrIdOrClassHandler(IApplicationModel model, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, ISelectedValueSelectorHandler selectedValueSelectorHandler,
            IDictionary<ScriptStepType, IScriptStepLogic> scriptStepLogicDictionary) {
        _Model = model;
        _GuiAndAppHandler = guiAndAppHandler;
        _SelectedValueSelectorHandler = selectedValueSelectorHandler;
        _ScriptStepLogicDictionary = scriptStepLogicDictionary;
    }

    public async Task EnableOrDisableFormOrControlOrIdOrClassAndSetLabelTextAsync() {
        var scriptStepType = _Model.ScriptStepType.SelectionMade ? (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid) : ScriptStepType.EndOfScript;
        _ScriptStepLogicDictionary[scriptStepType].SetFormOrControlOrIdOrClassTitle();
        await UpdateSelectableFormsOrControlsOrIdsOrClassesAsync();
    }

    private async Task UpdateSelectableFormsOrControlsOrIdsOrClassesAsync() {
        _GuiAndAppHandler.IndicateBusy(true);
        var scriptStepType = _Model.ScriptStepType.SelectionMade ? (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid) : ScriptStepType.EndOfScript;
        var selectables = await _ScriptStepLogicDictionary[scriptStepType].SelectableFormsOrControlsOrIdsOrClassesAsync();
        if (_Model.FormOrControlOrIdOrClass.AreSelectablesIdentical(selectables)) { return; }

        _Model.FormOrControlOrIdOrClass.UpdateSelectables(selectables);
        if (_Model.FormOrControlOrIdOrClass.SelectionMade) {
            await FormOrControlOrIdOrClassSelectedIndexChangedAsync(_Model.FormOrControlOrIdOrClass.SelectedIndex, true);
        } else {
            await FormOrControlOrIdOrClassSelectedIndexChangedAsync(_Model.FormOrControlOrIdOrClass.Selectables.Any() ? 0 : -1, true);
        }
    }

    public async Task FormOrIdOrClassInstanceNumberChangedAsync(string text) {
        if (_Model.FormOrIdOrClassInstanceNumber.Text == text) { return; }

        _Model.FormOrIdOrClassInstanceNumber.Text = text;
        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }

    public async Task FormOrControlOrIdOrClassSelectedIndexChangedAsync(int selectedIndex, bool selectablesChanged) {
        if (!selectablesChanged && _Model.FormOrControlOrIdOrClass.SelectedIndex == selectedIndex) { return; }

        _Model.FormOrControlOrIdOrClass.SelectedIndex = selectedIndex;
        var scriptStepType = _Model.ScriptStepType.SelectionMade && _Model.ScriptStepType.SelectedItem != null ? (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid) : ScriptStepType.EndOfScript;
        _Model.ScriptStepOucoOrOutrapForm = null;
        _Model.ScriptStepOutOfControl = null;
        _Model.ScriptStepIdOrClass = "";
        switch (scriptStepType) {
            case ScriptStepType.With:
                _Model.ScriptStepOucoOrOutrapForm = _Model.FormOrControlOrIdOrClass.SelectedItem;
                break;
            case ScriptStepType.Recognize:
            case ScriptStepType.NotExpectedContents:
            case ScriptStepType.Check:
            case ScriptStepType.Uncheck:
            case ScriptStepType.Press:
            case ScriptStepType.Input:
            case ScriptStepType.Select:
            case ScriptStepType.RecognizeSelection:
            case ScriptStepType.NotExpectedSelection:
                _Model.ScriptStepOutOfControl = _Model.FormOrControlOrIdOrClass.SelectedItem;
                break;
            case ScriptStepType.WithIdOrClass:
            case ScriptStepType.NotExpectedIdOrClass:
                _Model.ScriptStepIdOrClass = _Model.FormOrControlOrIdOrClass.SelectionMade ? _Model.FormOrControlOrIdOrClass.SelectedItem.Guid : "";
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
                break;
            default:
                throw new NotImplementedException();
        }

        var anySelectables = _Model.FormOrControlOrIdOrClass.Selectables.Any();
        _Model.FormOrIdOrClassInstanceNumber.Enabled = anySelectables && _Model.FormOrControlOrIdOrClass.SelectedIndex > 0 && scriptStepType != ScriptStepType.Select;
        if (!_Model.FormOrIdOrClassInstanceNumber.Enabled) {
            await FormOrIdOrClassInstanceNumberChangedAsync("");
        } else if (string.IsNullOrWhiteSpace(_Model.FormOrIdOrClassInstanceNumber.Text)) {
            await FormOrIdOrClassInstanceNumberChangedAsync("1");
        }

        await _SelectedValueSelectorHandler.EnableOrDisableSelectedValueAsync();

        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }
}