namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface ISubScriptSelectorHandler {
    Task EnableOrDisableSubScriptAsync();
    Task SubScriptSelectedIndexChangedAsync(int selectedIndex, bool selectablesChanged);
}