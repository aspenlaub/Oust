using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.GUI;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

[TestClass]
public class OustWindowRemoteControlFailureTest : OustIntegrationTestBase {
    [TestInitialize]
    public async Task Initialize() {
        var container = (await new ContainerBuilder().UseVishizhukelNetWebAndPeghAsync("Oust", new DummyCsArgumentPrompter())).Build();
        var generator = new FlawedTestDataGenerator(new ContextFactory(), container.Resolve<ILogicalUrlRepository>());
        await generator.GenerateTestDataAsync();
    }

    [TestMethod]
    public async Task ProperErrorMessageWhenSelectedScriptIsSubScript() {
        var contextFactory = new ContextFactory();
        var obsoleteScriptChecker = new ObsoleteScriptChecker(new ApplicationModel(EnvironmentType.UnitTest), contextFactory);
        Assert.IsTrue(await obsoleteScriptChecker.IsTopLevelScriptObsoleteAsync(FlawedTestDataGenerator.SubScriptName));
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var task = sut.CreateSetValueTask(process, nameof(IApplicationModel.SelectedScript), FlawedTestDataGenerator.SubScriptName);
        var errorMessage = await sut.SubmitNewTaskAndAwaitCompletionAsync(task, false);
        Assert.AreEqual((object)Application.Properties.Resources.ScriptIsObsoleteOrSubScript, errorMessage);
    }

    [TestMethod]
    public async Task ProperErrorMessageWhenScriptProducesInvalidHtml() {
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = new List<ControllableProcessTask> {
            sut.CreateSetValueTask(process, nameof(IApplicationModel.SelectedScript), FlawedTestDataGenerator.FailingScriptName),
            sut.CreateVerifyValueTask(process, nameof(IApplicationModel.SelectedScript), FlawedTestDataGenerator.FailingScriptName)
        };
        await sut.RemotelyProcessTaskListAsync(process, tasks);
        tasks.Clear();
        await sut.RemotelyPressButtonAsync(process, nameof(OustWindow.Play), false);
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), Properties.Resources.AutoDestructSequenceHasBeenInitialized));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
    }
}