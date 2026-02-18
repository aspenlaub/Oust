using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.GUI;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Seoa.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

public class OustWindowUnderTest : OustWindowUnderTestActions, IDisposable {
    private EnvironmentType EnvironmentType => EnvironmentType.UnitTest;

    private readonly IFolderResolver _FolderResolver;
    private readonly IStarterAndStopper _OustStarterAndStopper;

    public OustWindowUnderTest(IFolderResolver folderResolver, ITashAccessor tashAccessor, IStarterAndStopper oustStarterAndStopper) : base(tashAccessor) {
        _FolderResolver = folderResolver;
        _OustStarterAndStopper = oustStarterAndStopper;
        var errorsAndInfos = new ErrorsAndInfos();
        CleanupUnitTestDumpFolder(errorsAndInfos);
        Assert.That.ThereWereNoErrors(errorsAndInfos);
    }

    public override async Task InitializeAsync() {
        await base.InitializeAsync();
        _OustStarterAndStopper.Start();
    }

    private void CleanupUnitTestDumpFolder(IErrorsAndInfos errorsAndInfos) {
        if (EnvironmentType != EnvironmentType.UnitTest) { return; }

        var dumpFolder = _FolderResolver.ResolveAsync("$(MainUserFolder)", errorsAndInfos).Result.SubFolder("Oust").SubFolder(Enum.GetName(typeof(EnvironmentType), EnvironmentType))
            .SubFolder("Dump");
        dumpFolder.CreateIfNecessary();
        if (errorsAndInfos.AnyErrors()) { return; }

        foreach (var fileName in Directory.GetFiles(dumpFolder.FullName, "*.xml")) {
            File.Delete(fileName);
        }
    }

    public List<ControllableProcessTask> CreateNewScriptTaskList(ControllableProcess process, string newScriptName) {
        return new() {
            CreateSetValueTask(process, nameof(IApplicationModel.SelectedScript), Script.NewScriptName),
            CreateSetValueTask(process, nameof(IApplicationModel.NewScriptName), newScriptName),
            CreatePressButtonTask(process, nameof(OustWindow.RenameScript)),
            CreateVerifyValueTask(process, nameof(IApplicationModel.SelectedScript), newScriptName)
        };
    }

    public void Dispose() {
        _OustStarterAndStopper.Stop();
        Thread.Sleep(TimeSpan.FromSeconds(2));
        var errorsAndInfos = new ErrorsAndInfos();
        CleanupUnitTestDumpFolder(errorsAndInfos);
        Assert.That.ThereWereNoErrors(errorsAndInfos);
    }
}