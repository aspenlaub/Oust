using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Enums;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

public class OustWindowUnderTestActions : WindowUnderTestActionsBase {
    public OustWindowUnderTestActions(ITashAccessor tashAccessor) : base(tashAccessor, "Aspenlaub.Net.GitHub.CSharp.Oust") {
    }

    public ControllableProcessTask CreateSelectScriptTask(ControllableProcess process, string scriptName) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.SelectComboItem, nameof(IApplicationModel.SelectedScript), scriptName);
    }
}