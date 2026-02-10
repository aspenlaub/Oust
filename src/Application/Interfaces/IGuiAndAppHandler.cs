using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IGuiAndAppHandler : IGuiAndWebViewAppHandler<ApplicationModel> {
    Task UpdateFreeCodeLabelTextAsync();
}