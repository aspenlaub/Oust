using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.GUI;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Seoa.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

[TestClass]
public class OustWindowUploadTest : OustIntegrationTestBase {
    [TestMethod]
    public async Task CanUseUploadControl() {
        const string fileName = @"c:\temp\LoadMeUp.txt";
        if (!File.Exists(fileName)) {
            await File.WriteAllTextAsync(fileName, fileName);
        }

        IContainer container = new ContainerBuilder().UseDvinAndPegh("Oust").Build();
        var helper = new OustSettingsHelper(container.Resolve<ISecretRepository>());
        var errorsAndInfos = new ErrorsAndInfos();
        bool shouldWindows11BeAssumed = (await helper.ShouldWindows11BeAssumedAsync(errorsAndInfos)).YesNo;
        Assert.That.ThereWereNoErrors(errorsAndInfos);
        if (shouldWindows11BeAssumed) {
            Assert.Inconclusive("Upload currently is bypassed in Windows 11, therefore not tested here");
        }

        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();

        using OustWindowUnderTest sut = await CreateOustWindowUnderTestAsync();
        ControllableProcess process = await sut.FindIdleProcessAsync();
        List<ControllableProcessTask> tasks = CreateNewScriptTaskList(sut, process, "Load Me Up!");
        tasks.AddRange(await CreateGoToGutLookStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.With)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "GutLookForm"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrIdOrClassInstanceNumber), "1"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Recognize)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "UploadControl (Upload)"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Input)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "UploadControl (Upload)"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FreeText), fileName));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Recognize)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "UploadControl (Upload)"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FreeText), "0%"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Press)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "UploadControl (Upload)"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
        tasks.Clear();

        await Task.Delay(TimeSpan.FromSeconds(10));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.NotExpectedContents)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "UploadControl (Upload)"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FreeText), "0%"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }
}