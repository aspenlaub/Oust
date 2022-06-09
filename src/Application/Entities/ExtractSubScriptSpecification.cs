using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;

public class ExtractSubScriptSpecification : IExtractSubScriptSpecification {
    public string SubScriptName { get; }
    public List<Selectable> StepsToExtract { get; }

    public ExtractSubScriptSpecification(string subScriptName, IEnumerable<Selectable> stepsToExtract) {
        SubScriptName = subScriptName;
        StepsToExtract = new List<Selectable>(stepsToExtract);
    }
}