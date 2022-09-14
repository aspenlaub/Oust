namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface ISelectScriptFromListPopup {
    string ShowDialog(IList<string> selectableScriptNames);
}