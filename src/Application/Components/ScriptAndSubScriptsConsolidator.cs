using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.EntityFrameworkCore;

// ReSharper disable LoopCanBeConvertedToQuery

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;

public class ScriptAndSubScriptsConsolidator : IScriptAndSubScriptsConsolidator {
    private readonly IContextFactory _ContextFactory;

    public ScriptAndSubScriptsConsolidator(IContextFactory contextFactory) {
        _ContextFactory = contextFactory;
    }

    public async Task<List<Script>> GetSubScriptsOrderedByDescendingSizeAsync(EnvironmentType environmentType) {
        await using var context = await _ContextFactory.CreateAsync(environmentType);
        var subScriptGuids = await context.Scripts.Include(s => s.ScriptSteps).SelectMany(s => s.ScriptSteps.Where(r => r.ScriptStepType == ScriptStepType.SubScript).Select(r => r.SubScriptGuid)).Distinct().ToListAsync();
        var subScripts = await context.Scripts.Include(s => s.ScriptSteps).Where(s => subScriptGuids.Contains(s.Guid)).OrderByDescending(s => s.ScriptSteps.Count).ToListAsync();
        return subScripts;
    }

    public async Task<List<Script>> GetScriptsForWhichSubScriptCouldBeUsedAsync(EnvironmentType environmentType, Script subScript) {
        await using var context = await _ContextFactory.CreateAsync(environmentType);
        var formattedOrderedSubScriptSteps = string.Join("\t", subScript.OrderedScriptSteps().Where(s => s.ScriptStepType != ScriptStepType.EndOfScript).Select(s => s.ToString()));
        var result = new List<Script>();
        foreach (var script in context.Scripts.Where(s => s.Guid != subScript.Guid && s.ScriptSteps.Count > subScript.ScriptSteps.Count).Include(s => s.ScriptSteps)) {
            var formattedOrderedScriptSteps = string.Join("\t", script.OrderedScriptSteps().Select(s => s.ToString()));
            if (!formattedOrderedScriptSteps.Contains(formattedOrderedSubScriptSteps)) { continue; }

            result.Add(script);
        }
        return result;
    }
}