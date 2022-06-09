using Aspenlaub.Net.GitHub.CSharp.Oust.Application;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.GUI;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.GUI;

public class GuiToApplicationGate : GuiToWebViewApplicationGateBase<Application.Application, ApplicationModel> {
    public GuiToApplicationGate(IBusy busy, Application.Application application) : base(busy, application) {
    }
}