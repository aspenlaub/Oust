using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Extensions;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Helpers;

public class FileDialogTrickster : IFileDialogTrickster {
    private const string ResponseFile = @"c:\temp\pressenter.log";

    private readonly IFolderResolver _FolderResolver;

    public FileDialogTrickster(IFolderResolver folderResolver) {
        _FolderResolver = folderResolver;
    }

    public async Task<string> EnterFileNameAndHaveOpenButtonPressedReturnErrorMessageAsync(string fileName, string windowName) {
        if (!File.Exists(fileName)) {
            return string.Format(Properties.Resources.FileDoesNotExist, fileName);
        }

        var errorMessage = await LaunchPalefaceIfNecessaryReturnErrorMessageAsync();
        if (!string.IsNullOrEmpty(errorMessage)) { return errorMessage; }

        var errorsAndInfos = new ErrorsAndInfos();
        var folder = await _FolderResolver.ResolveAsync(@"$(GitHub)\PressEnterBin\Release", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            return errorsAndInfos.ErrorsToString();
        }

        var executable = folder.FullName + @"\Aspenlaub.Net.GitHub.CSharp.PressEnter.exe";
        if (!File.Exists(executable)) {
            return string.Format(Properties.Resources.FileDoesNotExist, executable);
        }

        WaitUntil(CouldResetResponseFile, 10000);

        await Task.Run(() => LaunchPressEnterProcess(executable, fileName, ResponseFile, windowName));

        WaitUntil(() => File.Exists(ResponseFile) && File.ReadAllText(ResponseFile).Length > 10, 300000);
        errorMessage = File.Exists(ResponseFile) ? await File.ReadAllTextAsync(ResponseFile) : Properties.Resources.OpenButtonWasNotPressed;

        return errorMessage != Properties.Resources.FileNameHasBeenEntered ? errorMessage : "";
    }

    private static bool CouldResetResponseFile() {
        try {
            File.WriteAllText(ResponseFile, "");
            return true;
        } catch {
            return false;
        }
    }

    private static void WaitUntil(Func<bool> condition, int milliseconds) {
        var internalMilliseconds = 1 + milliseconds / 20;
        do {
            if (condition()) { return; }

            Thread.Sleep(internalMilliseconds); // Do not use await Task.Delay here
            milliseconds = milliseconds - internalMilliseconds;
        } while (milliseconds >= 0);

    }

    private static void LaunchPressEnterProcess(string executable, string fileName, string responseFile, string windowName) {
        var p = new Process {
            StartInfo = {
                FileName = executable,
                Arguments = "4 \"" + fileName + "\" \"" + responseFile + "\" \"" + windowName + "\""
            }
        };
        p.Start();
    }

    private async Task<string> LaunchPalefaceIfNecessaryReturnErrorMessageAsync() {
        if (Process.GetProcessesByName("Aspenlaub.Net.GitHub.CSharp.Paleface").Length == 1) { return ""; }

        const string folderName = @"$(GitHub)\PalefaceBin\Release";
        var errorsAndInfos = new ErrorsAndInfos();
        var folder = await _FolderResolver.ResolveAsync(folderName, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            return errorsAndInfos.ErrorsToString();
        }

        var executable = folder.FullName + @"\Aspenlaub.Net.GitHub.CSharp.Paleface.exe";
        if (!File.Exists(executable)) {
            return "Paleface process could not be started";
        }

        var p = new Process {
            StartInfo = {FileName = executable}
        };
        p.Start();
        Thread.Sleep(TimeSpan.FromSeconds(5));
        return Process.GetProcessesByName("Aspenlaub.Net.GitHub.CSharp.Paleface").Length == 1 ? "" : "Paleface process started but could not be found";
    }
}