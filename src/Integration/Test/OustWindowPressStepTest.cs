using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.GUI;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

[TestClass]
public class OustWindowPressStepTest : OustIntegrationTestBase {
    [TestMethod]
    public async Task CanPress() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = CreateNewScriptTaskList(sut, process, "Tell Me To Press");
        tasks.AddRange(await CreateGoToGutLookStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Press)));
        tasks.Add(sut.CreateVerifyWhetherEnabledTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), false));
        tasks.AddRange(CreateWithGutLookSubFormTaskList(sut, process));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
        tasks.Clear();

        var paragraphsBefore = (await sut.RemotelyGetValueAsync(process, nameof(IApplicationModel.WebViewParagraphs))).Split('\t').ToList();
        Assert.HasCount(11, paragraphsBefore);
        await Task.Delay(2000);

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Press)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "SubButton (Button)"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
        tasks.Clear();

        var success = false;
        var paragraphsAfter = new List<string>();
        for (var i = 0; i < 200 && !success; i++) {
            await Task.Delay(50);
            paragraphsAfter = (await sut.RemotelyGetValueAsync(process, nameof(IApplicationModel.WebViewParagraphs))).Split('\t').ToList();
            Assert.HasCount(11, paragraphsAfter);
            success = paragraphsBefore[3] != paragraphsAfter[3];
        }
        Assert.IsTrue(success);

        Assert.AreEqual(paragraphsBefore[0], paragraphsAfter[0]);
        Assert.AreEqual(paragraphsBefore[1], paragraphsAfter[1]);
        Assert.AreEqual(paragraphsBefore[2], paragraphsAfter[2]);
        Assert.AreNotEqual(paragraphsBefore[3], paragraphsAfter[3]);
        Assert.AreEqual(paragraphsBefore[4], paragraphsAfter[4]);
        Assert.AreNotEqual(paragraphsBefore[5], paragraphsAfter[5]);
        Assert.AreEqual(paragraphsBefore[6], paragraphsAfter[6]);
        Assert.AreEqual(paragraphsBefore[7], paragraphsAfter[7]);
        Assert.AreEqual(paragraphsBefore[8], paragraphsAfter[8]);
        Assert.AreEqual(paragraphsBefore[9], paragraphsAfter[9]);
        Assert.AreEqual(paragraphsBefore[10], paragraphsAfter[10]);
    }

    [TestMethod]
    public async Task PressCanChangeUrl() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = CreateNewScriptTaskList(sut, process, "Press To Take Me Further");

        var errorsAndInfos = new ErrorsAndInfos();
        var url = await LogicalUrlRepository.GetUrlAsync("Brainstick", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        tasks.AddRange(CreateGoToStepTaskList(sut, process, url));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.WithIdOrClass)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), ".imagemenu"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrIdOrClassInstanceNumber), "4"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.PressSingle)));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.WebViewUrl), url + "?brainsource=main&brainmode=5"));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
        tasks.Clear();
    }
}