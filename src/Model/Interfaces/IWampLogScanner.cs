using System;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;

public interface IWampLogScanner {
    DateTime WaitUntilLogFolderIsErrorFreeReturnStartOfExecutionTimeStamp();
    void WaitUntilLogFolderIsStable(DateTime startOfExecutionTimeStamp, out string errorMessage);
}