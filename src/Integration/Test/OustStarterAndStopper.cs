using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

public class OustStarterAndStopper : StarterAndStopperBase {
    protected override string ProcessName => "Aspenlaub.Net.GitHub.CSharp.Oust";
    protected override List<string> AdditionalProcessNamesToStop => new() {"Aspenlaub.Net.GitHub.CSharp.PressEnter"};
    protected override string ExecutableFile() {
        return typeof(OustWindowUnderTest).Assembly.Location
            .Replace(@"\Integration\Test\", @"\")
            .Replace("Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test.dll", ProcessName + ".exe");
    }
}