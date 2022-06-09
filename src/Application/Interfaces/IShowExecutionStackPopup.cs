namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IShowExecutionStackPopup {
    void ShowDialog(IList<string> formattedExecutionStack);
}