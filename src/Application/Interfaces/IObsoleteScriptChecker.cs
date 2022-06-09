namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IObsoleteScriptChecker {
    Task<bool> IsTopLevelScriptObsoleteAsync(string name);
}