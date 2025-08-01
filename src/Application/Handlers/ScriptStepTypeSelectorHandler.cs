using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class ScriptStepTypeSelectorHandler(IApplicationModel model, IGuiAndAppHandler guiAndAppHandler,
        IFormOrControlOrIdOrClassHandler formOrControlOrIdOrClassHandler, ISelectedValueSelectorHandler selectedValueSelectorHandler,
        ISubScriptSelectorHandler subScriptSelectorHandler) : IScriptStepTypeSelectorHandler {
    public void UpdateSelectableScriptStepTypes() {
        var choices = new List<ScriptStepType> {
            ScriptStepType.GoToUrl, ScriptStepType.With, ScriptStepType.WithIdOrClass, ScriptStepType.NotExpectedIdOrClass,
            ScriptStepType.Recognize, ScriptStepType.RecognizeSelection, ScriptStepType.NotExpectedContents, ScriptStepType.NotExpectedSelection,
            ScriptStepType.Check, ScriptStepType.CheckSingle, ScriptStepType.Uncheck, ScriptStepType.UncheckSingle,
            ScriptStepType.Press, ScriptStepType.PressSingle, ScriptStepType.Input, ScriptStepType.InputIntoSingle, ScriptStepType.Select,
            ScriptStepType.SubScript, ScriptStepType.WaitAMinute, ScriptStepType.EndOfScript, ScriptStepType.WaitTenSeconds, ScriptStepType.InvokeUrl,
            ScriptStepType.RecognizeOkay, ScriptStepType.ClearInput, ScriptStepType.EndScriptIfRecognized, ScriptStepType.StartOfCleanUpSection
        };
        model.ScriptStepType.Selectables.Clear();
        choices.ForEach(c => model.ScriptStepType.Selectables.Add(new Selectable { Guid = ((int)c).ToString(), Name = Enum.GetName(typeof(ScriptStepType), c) }));
    }

    public async Task ScriptStepTypeSelectedIndexChangedAsync(int scriptStepTypeSelectedIndex, bool clearStepDetails) {
        if (model.ScriptStepType.SelectedIndex == scriptStepTypeSelectedIndex) { return; }

        model.ScriptStepType.SelectedIndex = scriptStepTypeSelectedIndex;
        await guiAndAppHandler.UpdateFreeCodeLabelTextAsync();
        await formOrControlOrIdOrClassHandler.EnableOrDisableFormOrControlOrIdOrClassAndSetLabelTextAsync();
        await selectedValueSelectorHandler.EnableOrDisableSelectedValueAsync();
        await subScriptSelectorHandler.EnableOrDisableSubScriptAsync();
        if (clearStepDetails || scriptStepTypeSelectedIndex < 0) {
            model.FreeText.Text = "";
            model.FormOrIdOrClassInstanceNumber.Text = "";
        }

        await guiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }
}