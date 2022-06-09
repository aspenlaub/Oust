using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IExtractSubScriptSpecification {
    string SubScriptName { get; }
    List<Selectable> StepsToExtract { get; }
}