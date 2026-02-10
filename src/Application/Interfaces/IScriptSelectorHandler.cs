using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IScriptSelectorHandler {
    Task EnsureNewScriptAsync();
    Task UpdateSelectableScriptsAsync();
    Task SelectedScriptSelectedIndexChangedAsync(int selectedScriptSelectedIndex, bool resetExecutionStack);
}