using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.IO;

public class WampLogScanner : IWampLogScanner {
    protected IFolder LogFolder, ExtendedLogFolder, WampLogFolder;
    protected Dictionary<string, int> Snapshot;

    public WampLogScanner(IFolder logFolder, IFolder extendedLogFolder, IFolder wampLogFolder) {
        LogFolder = logFolder;
        ExtendedLogFolder = extendedLogFolder;
        WampLogFolder = wampLogFolder;
        SaveSnapshots();
    }

    protected Dictionary<string, int> ScanFolder() {
        var snapshot = new Dictionary<string, int>();
        foreach (var fileName in Directory.GetFiles(LogFolder.FullName, "*-ERR.log").OrderBy(f => f)) {
            snapshot[fileName] = File.ReadAllText(fileName).Length;
        }

        return snapshot;

    }

    protected void SaveSnapshots() {
        Snapshot = ScanFolder();
    }

    private string LastNewErrorMessage() {
        var newErrors = new List<string>();
        var snapshot = ScanFolder();
        const string errorTag = "ERROR: ";
        foreach (var fileName in snapshot.Keys) {
            var oldLength = Snapshot.TryGetValue(fileName, out var value) ? value : 0;
            newErrors.AddRange(File.ReadAllText(fileName).Substring(oldLength).Split('\n').Where(s => s.Contains(errorTag)).Select(s => s.Substring(s.IndexOf(errorTag, StringComparison.Ordinal) + errorTag.Length).Replace("\r", "")).ToList());
        }

        SaveSnapshots();
        return !newErrors.Any() ? "" : newErrors.Last();
    }

    private DateTime NewestFileChangeTimeStamp(DateTime minTimeStamp) {
        return Directory.GetFiles(LogFolder.FullName, "*.log").Select(File.GetLastWriteTime)
            .Union(Directory.GetFiles(ExtendedLogFolder.FullName, "*.log").Select(File.GetLastWriteTime))
            .Union(new[] { minTimeStamp }).Max();
    }

    public void WaitUntilLogFolderIsStable(DateTime startOfExecutionTimeStamp, out string errorMessage) {
        const int maxSeconds = 100;
        var intervalInMilliseconds = 500;
        var attempts = maxSeconds * 1000 / intervalInMilliseconds;
        DateTime lastTimeStamp, newTimeStamp = DateTime.Now;
        do {
            lastTimeStamp = newTimeStamp;
            Thread.Sleep(intervalInMilliseconds);
            intervalInMilliseconds = 2000;
            newTimeStamp = NewestFileChangeTimeStamp(lastTimeStamp);
            attempts--;
            errorMessage = LastNewErrorMessage();
            if (errorMessage != "") { return; }
        } while (attempts >= 0 && lastTimeStamp != newTimeStamp);

        errorMessage = LatestPhpErrorSince(startOfExecutionTimeStamp);
    }

    protected string LatestPhpErrorSince(DateTime executionTimeStamp) {
        if (!WampLogFolder.Exists()) {
            return $"Wamp log folder does not exist: {WampLogFolder.FullName}";
        }

        var fileName = WampLogFolder.FullName + @"\php_error.log";
        if (!File.Exists(fileName)) {
            return $"Wamp php error log does not exist: {fileName}";
        }

        return File.GetLastWriteTime(fileName) < executionTimeStamp ? "" : File.ReadAllLines(fileName).Last();
    }
}