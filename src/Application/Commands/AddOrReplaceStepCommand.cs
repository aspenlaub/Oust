using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class AddOrReplaceStepCommand(IApplicationModel model, IScriptStepSelectorHandler scriptStepSelectorHandler,
            IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, ICommand stepIntoCommand,
            IDictionary<ScriptStepType, IScriptStepLogic> scriptStepLogicDictionary, IContextFactory contextFactory)
                : ICommand {
    public async Task ExecuteAsync() {
        if (!model.AddOrReplaceStep.Enabled) { return; }

        var scriptStepType = (ScriptStepType)int.Parse(model.ScriptStepType.SelectedItem.Guid);
        if (!scriptStepLogicDictionary.ContainsKey(scriptStepType)) {
            return;
        }

        var newOrUpdatedScriptStep = scriptStepLogicDictionary[scriptStepType].CreateScriptStepToAdd() as ScriptStep;
        if (newOrUpdatedScriptStep == null) { return; }

        await using (Context context = await contextFactory.CreateAsync(model.EnvironmentType)) {
            Script script = await context.LoadScriptWithStepsAsync(model.SelectedScript.SelectedItem.Guid);
            if (!model.ScriptSteps.SelectionMade || IsLastScriptStepSelected()) {
                int stepNumber = 1 + (script.ScriptSteps.Any() ? script.ScriptSteps.Max(s => s.StepNumber) : 0);
                newOrUpdatedScriptStep.StepNumber = stepNumber;
                script.ScriptSteps.Add(newOrUpdatedScriptStep);
            } else {
                ScriptStep existingStep = script.ScriptSteps.First(s => s.Guid == model.ScriptSteps.Selectables[model.ScriptSteps.SelectedIndex].Guid);
                existingStep.ReplaceWith(newOrUpdatedScriptStep);
            }
            await context.SaveChangesAsync();
        }

        await scriptStepSelectorHandler.UpdateSelectableScriptStepsAfterCurrentHasBeenAddedAsync();
        if (scriptStepType != (ScriptStepType) int.Parse(model.ScriptStepType.SelectedItem.Guid)) {
            throw new Exception("Unexpected script step type");
        }
        await guiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
        if (scriptStepType != ScriptStepType.WaitAMinute
                && scriptStepType != ScriptStepType.SubScript
                && scriptStepType != ScriptStepType.WaitTenSeconds) {
            await stepIntoCommand.ExecuteAsync();
        }
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        if (!model.SelectedScript.SelectionMade) { return false; }
        if (model.SelectedScript.SelectedItem.Name == Script.NewScriptName) { return false; }
        if (!model.ScriptStepType.SelectionMade) { return false; }

        var scriptStepType = (ScriptStepType)int.Parse(model.ScriptStepType.SelectedItem.Guid);
        if (!scriptStepLogicDictionary.ContainsKey(scriptStepType)) {
            return false;
        }

        return await scriptStepLogicDictionary[scriptStepType].CanBeAddedOrReplaceExistingStepAsync();
    }

    private bool IsLastScriptStepSelected() {
        return model.ScriptSteps.SelectionMade && model.ScriptSteps.SelectedIndex == model.ScriptSteps.Selectables.Count - 1;
    }
}