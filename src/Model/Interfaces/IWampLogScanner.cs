namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;

public interface IWampLogScanner {
    void WaitUntilLogFolderIsStable(DateTime startOfExecutionTimeStamp, out string errorMessage);
}