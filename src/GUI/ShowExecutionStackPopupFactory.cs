using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.GUI;

public class ShowExecutionStackPopupFactory : IShowExecutionStackPopupFactory {
    public IShowExecutionStackPopup Create() {
        return new ShowExecutionStackPopup();
    }
}