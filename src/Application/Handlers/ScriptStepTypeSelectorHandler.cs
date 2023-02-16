using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class ScriptStepTypeSelectorHandler : IScriptStepTypeSelectorHandler {
    private readonly IApplicationModel _Model;
    private readonly IGuiAndAppHandler _GuiAndAppHandler;
    private readonly IFormOrControlOrIdOrClassHandler _FormOrControlOrIdOrClassHandler;
    private readonly ISelectedValueSelectorHandler _SelectedValueSelectorHandler;
    private readonly ISubScriptSelectorHandler _SubScriptSelectorHandler;

    public ScriptStepTypeSelectorHandler(IApplicationModel model, IGuiAndAppHandler guiAndAppHandler, IFormOrControlOrIdOrClassHandler formOrControlOrIdOrClassHandler,
        ISelectedValueSelectorHandler selectedValueSelectorHandler, ISubScriptSelectorHandler subScriptSelectorHandler) {
        _Model = model;
        _GuiAndAppHandler = guiAndAppHandler;
        _FormOrControlOrIdOrClassHandler = formOrControlOrIdOrClassHandler;
        _SelectedValueSelectorHandler = selectedValueSelectorHandler;
        _SubScriptSelectorHandler = subScriptSelectorHandler;
    }

    public void UpdateSelectableScriptStepTypes() {
        var choices = new List<ScriptStepType> {
            ScriptStepType.GoToUrl, ScriptStepType.With, ScriptStepType.WithIdOrClass, ScriptStepType.NotExpectedIdOrClass,
            ScriptStepType.Recognize, ScriptStepType.RecognizeSelection, ScriptStepType.NotExpectedContents, ScriptStepType.NotExpectedSelection,
            ScriptStepType.Check, ScriptStepType.CheckSingle, ScriptStepType.Uncheck, ScriptStepType.UncheckSingle,
            ScriptStepType.Press, ScriptStepType.PressSingle, ScriptStepType.Input, ScriptStepType.InputIntoSingle, ScriptStepType.Select,
            ScriptStepType.SubScript, ScriptStepType.WaitAMinute, ScriptStepType.EndOfScript, ScriptStepType.WaitTenSeconds, ScriptStepType.InvokeUrl,
            ScriptStepType.RecognizeOkay, ScriptStepType.ClearInput
        };
        _Model.ScriptStepType.Selectables.Clear();
        choices.ForEach(c => _Model.ScriptStepType.Selectables.Add(new Selectable { Guid = ((int)c).ToString(), Name = Enum.GetName(typeof(ScriptStepType), c) }));
    }

    public async Task ScriptStepTypeSelectedIndexChangedAsync(int scriptStepTypeSelectedIndex, bool clearStepDetails) {
        if (_Model.ScriptStepType.SelectedIndex == scriptStepTypeSelectedIndex) { return; }

        _Model.ScriptStepType.SelectedIndex = scriptStepTypeSelectedIndex;
        await _GuiAndAppHandler.UpdateFreeCodeLabelTextAsync();
        await _FormOrControlOrIdOrClassHandler.EnableOrDisableFormOrControlOrIdOrClassAndSetLabelTextAsync();
        await _SelectedValueSelectorHandler.EnableOrDisableSelectedValueAsync();
        await _SubScriptSelectorHandler.EnableOrDisableSubScriptAsync();
        if (clearStepDetails || scriptStepTypeSelectedIndex < 0) {
            _Model.FreeText.Text = "";
            _Model.FormOrIdOrClassInstanceNumber.Text = "";
        }

        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }
}