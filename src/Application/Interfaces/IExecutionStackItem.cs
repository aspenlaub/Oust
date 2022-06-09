namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IExecutionStackItem {
    string ScriptGuid { get; }
    string ScriptName { get; }
    string ScriptStepGuid { get; }
}