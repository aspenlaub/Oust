using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Extensions;

public static class ScriptExtensions {
    public static bool ProperlyEndReturnIfChanged(this Script script) {
        if (script.Name == Script.NewScriptName) { return false; }

        var orderedScriptSteps = script.OrderedScriptSteps();
        var changed = false;
        var stepsToRemove = new List<ScriptStep>();
        for (var i = 0; i < orderedScriptSteps.Count - 1; i++) {
            if (orderedScriptSteps[i].ScriptStepType != ScriptStepType.EndOfScript) { continue; }
            stepsToRemove.Add(orderedScriptSteps[i]);
        }
        foreach (var scriptStep in stepsToRemove) {
            script.ScriptSteps.Remove(scriptStep);
            changed = true;
        }

        if (orderedScriptSteps.Count != 0 && orderedScriptSteps[^1].ScriptStepType == ScriptStepType.EndOfScript) {
            return changed;
        }

        script.ScriptSteps.Add(new ScriptStep { ScriptStepType = ScriptStepType.EndOfScript, StepNumber = script.ScriptSteps.Count == 0 ? 1 : script.ScriptSteps.Max(s => s.StepNumber + 1) });
        return true;
    }
}