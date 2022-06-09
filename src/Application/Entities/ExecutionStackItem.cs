using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;

public class ExecutionStackItem : IExecutionStackItem {
    public string ScriptGuid { get; }
    public string ScriptName { get; }
    public string ScriptStepGuid { get; }
    public bool StopAfterSubScript { get; }

    public ExecutionStackItem(string scriptGuid, string scriptName, string scriptStepGuid, bool stopAfterSubScript) {
        ScriptGuid = scriptGuid;
        ScriptName = scriptName;
        ScriptStepGuid = scriptStepGuid;
        StopAfterSubScript = stopAfterSubScript;
    }
}