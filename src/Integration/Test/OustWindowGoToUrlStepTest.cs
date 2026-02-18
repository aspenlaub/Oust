using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.GUI;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Seoa.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

[TestClass]
public class OustWindowGoToUrlStepTest : OustIntegrationTestBase {
    [TestInitialize]
    public async Task Initialize() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
    }

    [TestMethod]
    public async Task FreeTextForGoToUrlStepIsLabeledUrl() {
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        await sut.RemotelySetValueAsync(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.GoToUrl));
        await sut.RemotelyVerifyLabelAsync(process, nameof(IApplicationModel.FreeText), Properties.Resources.FreeTextUrl);
    }

    [TestMethod]
    public async Task CanAddAndReplaceGoToUrlSteps() {
        await CallGutLookOnceIgnoreResultAsync();

        var errorsAndInfos = new ErrorsAndInfos();
        var url = await LogicalUrlRepository.GetUrlAsync("GutLookForms", errorsAndInfos);
        Assert.That.ThereWereNoErrors(errorsAndInfos);

        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = CreateNewScriptTaskList(sut, process, "Can Add GoTo Url Steps");
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.GoToUrl)));
        var stepUrls = new List<string>();
        for (var i = 0; i < 3; i ++) {
            var stepUrl = url + "&n=" + i;
            stepUrls.Add(stepUrl);
            tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FreeText), stepUrl));
            tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
            tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.ScriptSteps), i + 1));
        }
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), "\u2192 " + stepUrls[1]));
        var newUrl = url + "&replaced=y";
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FreeText), newUrl));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), "\u2192 " + stepUrls[0]));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), "\u2192 " + newUrl));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }

    [TestMethod]
    public async Task StopsOnErrorLog() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = sut.CreateNewScriptTaskList(process, "Stops On Error Log");
        tasks.AddRange(await CreateGoToErrorLogOnWrongCountTaskListAsync(sut, process, 0));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        var addTasks = await CreateGoToErrorLogOnWrongCountTaskListAsync(sut, process, 1);
        tasks.AddRange(addTasks);
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        var secondStepText = "\u2192 " + addTasks[1].Text;
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), secondStepText));
        tasks.AddRange(await CreateGoToErrorLogOnWrongCountTaskListAsync(sut, process, 2));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), secondStepText));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);

        await StepIntoAsync(sut, process, false);

        tasks.Clear();
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), "n is invalid, you are counting in a wrong way"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), secondStepText));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }

    [TestMethod]
    public async Task StopsOnInvalidMarkup() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = sut.CreateNewScriptTaskList(process, "Stops On Invalid Markup");
        tasks.AddRange(await CreateGoToInvalidMarkUpOnWrongCountTaskListAsync(sut, process, 0));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        var addTasks = await CreateGoToInvalidMarkUpOnWrongCountTaskListAsync(sut, process, 1);
        tasks.AddRange(addTasks);
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        var secondStepText = "\u2192 " + addTasks[1].Text;
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), secondStepText));
        tasks.AddRange(await CreateGoToInvalidMarkUpOnWrongCountTaskListAsync(sut, process, 2));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), secondStepText));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);

        await StepIntoAsync(sut, process, false);

        tasks.Clear();
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), Properties.Resources.AutoDestructSequenceHasBeenInitialized));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), secondStepText));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }
}