using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.GUI;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

[TestClass]
public class OustWindowCodeCoverageTest : OustIntegrationTestBase {
    [TestInitialize]
    public async Task Initialize() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
    }

    [TestMethod]
    public async Task CanGetCodeCoverage() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        var folderResolver = Container.Resolve<IFolderResolver>();
        var dumperNameConverter = Container.Resolve<IDumperNameConverter>();
        var errorsAndInfos = new ErrorsAndInfos();
        var logFolder = await folderResolver.ResolveAsync(@"$(WampLog)", errorsAndInfos);
        var extendedLogFolder = await folderResolver.ResolveAsync(@"$(WampExtLog)", errorsAndInfos);
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = CreateNewScriptTaskList(sut, process, "Code Coverage");
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.CodeCoverage)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        var addTasks = await CreateGoToToughLookStepTaskListAsync(sut, process);
        tasks.AddRange(addTasks);
        var goToToughLookStepText = "\u2192 " + addTasks[1].Text;
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.StopCodeCoverage)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        var startTime = DateTime.Now;
        await sut.RemotelyProcessTaskListAsync(process, tasks);
        tasks.Clear();

        var resultFile = (await folderResolver.ResolveAsync(@"$(WampRoot)\temp\coverage", errorsAndInfos)).FullName + "\\" + dumperNameConverter.ScriptFileFriendlyShortName("Code Coverage") + ".txt";
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        Assert.IsTrue(File.Exists(resultFile));
        File.Delete(resultFile);
        Assert.AreEqual(6, FilesChangedSince(logFolder, extendedLogFolder, startTime));

        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.CodeCoverage)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToToughLookStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.StepInto)));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
        tasks.Clear();

        Assert.AreEqual(6, FilesChangedSince(logFolder, extendedLogFolder, startTime));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.StopCodeCoverage)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
        tasks.Clear();

        Assert.AreEqual(6, FilesChangedSince(logFolder, extendedLogFolder, startTime));
        Assert.IsTrue(File.Exists(resultFile));
        var lines = (await File.ReadAllLinesAsync(resultFile)).ToList();
        var line = lines.FirstOrDefault(l => l.Contains("toughlookframe.php"));
        Assert.IsNotNull(line);
        Assert.IsTrue(File.Exists(line));
        line = lines.FirstOrDefault(l => l.Contains("toughlookform.php"));
        Assert.IsNotNull(line);
        Assert.IsTrue(File.Exists(line));
        File.Delete(resultFile);
    }

    public int FilesChangedSince(IFolder logFolder, IFolder extendedLogFolder, DateTime timeStamp) {
        return Directory.GetFiles(logFolder.FullName, "localhost*.log")
            .Union(Directory.GetFiles(extendedLogFolder.FullName, "localhost*.log"))
            .Count(f => File.GetLastWriteTime(f) >= timeStamp);
    }
}