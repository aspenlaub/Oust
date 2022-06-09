using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class AddOrReplaceStepCommand : ICommand {
    private readonly IApplicationModel _Model;
    private readonly IScriptStepSelectorHandler _ScriptStepSelectorHandler;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;
    private readonly ICommand _StepIntoCommand;
    private readonly IDictionary<ScriptStepType, IScriptStepLogic> _ScriptStepLogicDictionary;
    private readonly IContextFactory _ContextFactory;

    public AddOrReplaceStepCommand(IApplicationModel model, IScriptStepSelectorHandler scriptStepSelectorHandler, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, ICommand stepIntoCommand, IDictionary<ScriptStepType, IScriptStepLogic> scriptStepLogicDictionary, IContextFactory contextFactory) {
        _Model = model;
        _ScriptStepLogicDictionary = scriptStepLogicDictionary;
        _ScriptStepSelectorHandler = scriptStepSelectorHandler;
        _GuiAndAppHandler = guiAndAppHandler;
        _StepIntoCommand = stepIntoCommand;
        _ContextFactory = contextFactory;
    }

    public async Task ExecuteAsync() {
        if (!_Model.AddOrReplaceStep.Enabled) { return; }

        var scriptStepType = (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid);
        if (!_ScriptStepLogicDictionary.ContainsKey(scriptStepType)) {
            return;
        }

        var newOrUpdatedScriptStep = _ScriptStepLogicDictionary[scriptStepType].CreateScriptStepToAdd() as ScriptStep;
        if (newOrUpdatedScriptStep == null) { return; }

        await using (var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType)) {
            var script = await context.LoadScriptWithStepsAsync(_Model.SelectedScript.SelectedItem.Guid);
            if (!_Model.ScriptSteps.SelectionMade || IsLastScriptStepSelected()) {
                var stepNumber = 1 + (script.ScriptSteps.Any() ? script.ScriptSteps.Max(s => s.StepNumber) : 0);
                newOrUpdatedScriptStep.StepNumber = stepNumber;
                script.ScriptSteps.Add(newOrUpdatedScriptStep);
            } else {
                var existingStep = script.ScriptSteps.First(s => s.Guid == _Model.ScriptSteps.Selectables[_Model.ScriptSteps.SelectedIndex].Guid);
                existingStep.ReplaceWith(newOrUpdatedScriptStep);
            }
            await context.SaveChangesAsync();
        }

        await _ScriptStepSelectorHandler.UpdateSelectableScriptStepsAfterCurrentHasBeenAddedAsync();
        if (scriptStepType != (ScriptStepType) int.Parse(_Model.ScriptStepType.SelectedItem.Guid)) {
            throw new Exception("Unexpected script step type");
        }
        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
        if (scriptStepType != ScriptStepType.WaitAMinute && scriptStepType != ScriptStepType.SubScript && scriptStepType != ScriptStepType.WaitTenSeconds) {
            await _StepIntoCommand.ExecuteAsync();
        }
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        if (!_Model.SelectedScript.SelectionMade) { return false; }
        if (_Model.SelectedScript.SelectedItem.Name == Script.NewScriptName) { return false; }
        if (!_Model.ScriptStepType.SelectionMade) { return false; }

        var scriptStepType = (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid);
        if (!_ScriptStepLogicDictionary.ContainsKey(scriptStepType)) {
            return false;
        }

        return await _ScriptStepLogicDictionary[scriptStepType].CanBeAddedOrReplaceExistingStepAsync();
    }

    private bool IsLastScriptStepSelected() {
        return _Model.ScriptSteps.SelectionMade && _Model.ScriptSteps.SelectedIndex == _Model.ScriptSteps.Selectables.Count - 1;
    }
}