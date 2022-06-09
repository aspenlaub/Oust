namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IFormOrControlOrIdOrClassHandler {
    Task EnableOrDisableFormOrControlOrIdOrClassAndSetLabelTextAsync();
    Task FormOrControlOrIdOrClassSelectedIndexChangedAsync(int selectedIndex, bool selectablesChanged);

    Task FormOrIdOrClassInstanceNumberChangedAsync(string text);
}