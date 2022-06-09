using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IExtractSubScriptPopup {
    IExtractSubScriptSpecification Show(ISelector selector);
    void OnApplicationShutdown();
}