namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface ISelectedValueSelectorHandler {
    Task EnableOrDisableSelectedValueAsync();
    Task SelectedValueSelectedIndexChangedAsync(int selectedIndex, bool selectablesChanged);
}