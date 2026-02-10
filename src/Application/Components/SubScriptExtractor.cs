using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;

public class SubScriptExtractor : ISubScriptExtractor {
    private readonly IContextFactory _ContextFactory;

    public SubScriptExtractor(IContextFactory contextFactory) {
        _ContextFactory = contextFactory;
    }

    public async Task<bool> ExtractSubScriptAsync(EnvironmentType environmentType, string scriptGuid, IExtractSubScriptSpecification extractSubScriptSpecification) {
        await using var context = await _ContextFactory.CreateAsync(environmentType);
        var script = await context.Scripts.Include(s => s.ScriptSteps).SingleOrDefaultAsync(s => s.Guid == scriptGuid);
        if (script == null) { return false; }

        var subScript = new Script {
            Name = extractSubScriptSpecification.SubScriptName
        };
        var stepOne = script.ScriptSteps.Single(s => s.Guid == extractSubScriptSpecification.StepsToExtract[0].Guid);
        var subScriptStep = new ScriptStep {
            ScriptStepType = ScriptStepType.SubScript,
            SubScriptGuid = subScript.Guid,
            SubScriptName = subScript.Name,
            StepNumber = stepOne.StepNumber
        };
        script.ScriptSteps.Add(subScriptStep);
        var stepNumber = 0;
        foreach (var step in extractSubScriptSpecification.StepsToExtract) {
            MoveStepToSubScript(script, subScript, step.Guid, ref stepNumber);
        }
        subScript.ScriptSteps.Add(new ScriptStep { ScriptStepType = ScriptStepType.EndOfScript, StepNumber = stepNumber });
        await context.Scripts.AddAsync(subScript);
        await context.SaveChangesAsync();
        return true;
    }

    private void MoveStepToSubScript(Script script, Script subScript, string stepGuid, ref int stepNumber) {
        var scriptStep = script.ScriptSteps.Single(s => s.Guid == stepGuid);
        script.ScriptSteps.Remove(scriptStep);
        subScript.ScriptSteps.Add(new ScriptStep(scriptStep) {
            StepNumber = stepNumber ++
        });
    }
}