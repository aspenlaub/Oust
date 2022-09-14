using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.GUI;

public class SelectScriptFromListPopupFactory : ISelectScriptFromListPopupFactory {
    public ISelectScriptFromListPopup Create() {
        return new SelectScriptFromListPopup();
    }
}