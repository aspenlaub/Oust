using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Helpers;

public class ExecutionStackFormatter : IExecutionStackFormatter {
    private readonly IContextFactory _ContextFactory;

    public ExecutionStackFormatter(IContextFactory contextFactory) {
        _ContextFactory = contextFactory;
    }
    public async Task<IList<string>>  FormatExecutionStackAsync(EnvironmentType environmentType, string currentScriptStepGuid, IList<IExecutionStackItem> stackItems) {
        var scriptNames = stackItems.Select(s => s.ScriptName).ToList();
        var scriptStepGuids = stackItems.Select(s => s.ScriptStepGuid).ToList();
        scriptNames.Insert(0, "");
        scriptStepGuids.Insert(0, currentScriptStepGuid);
        await using var context = await _ContextFactory.CreateAsync(environmentType);

        var scriptSteps = context.ScriptSteps.Where(s => scriptStepGuids.Contains(s.Guid)).ToList();
        return scriptStepGuids.Select((g, i) => FormatScriptStep(g, scriptNames[i], scriptSteps)).ToList();
    }

    protected string FormatScriptStep(string scriptStepGuid, string scriptName, IList<ScriptStep> scriptSteps) {
        var scriptStep = scriptSteps.First(step => step.Guid == scriptStepGuid);
        var s = scriptStep.ToString();
        if (string.IsNullOrWhiteSpace(scriptName)) { return s; }

        s += $"\r\nin \"{scriptName}\"";
        return s;
    }
}