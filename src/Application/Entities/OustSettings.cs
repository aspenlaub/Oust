using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;

[XmlRoot(nameof(OustSettings))]
public class OustSettings : List<OustSetting>, ISecretResult<OustSettings> {
    public OustSettings Clone() {
        var clone = new OustSettings();
        clone.AddRange(this);
        return clone;
    }

    public YesNoInconclusive ShouldWindows11BeAssumed() {
        var machine = Environment.MachineName.ToLower();
        var shouldWindows11BeAssumed = this.FirstOrDefault(f => f.Machine.ToLower() == machine)?.ShouldWindows11BeAssumed;
        return shouldWindows11BeAssumed == null
            ? new YesNoInconclusive { Inconclusive = true }
            : new YesNoInconclusive { Inconclusive = false, YesNo = (bool)shouldWindows11BeAssumed };
    }
}