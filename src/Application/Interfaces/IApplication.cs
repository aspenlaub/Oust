using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IApplication {
    IApplicationHandlers Handlers { get; }
    IApplicationCommands Commands { get; }
    ITashHandler<ApplicationModel> TashHandler { get; }

    Task OnLoadedAsync();
    Task OnWebViewWiredAsync();
    Task NewScriptNameChangedAsync(string text);
    Task FreeTextChangedAsync(string text);

    ITashTaskHandlingStatus<ApplicationModel> CreateTashTaskHandlingStatus();
}