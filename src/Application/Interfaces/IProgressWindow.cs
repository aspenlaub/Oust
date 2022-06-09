namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IProgressWindow {
    void Show(string caption);
    void AddMessage(string message);
    void OnApplicationShutdown();
}