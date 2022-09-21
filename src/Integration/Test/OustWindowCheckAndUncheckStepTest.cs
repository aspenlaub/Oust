using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

[TestClass]
public class OustWindowCheckAndUncheckStepTest : OustIntegrationTestBase {
    [TestMethod]
    public async Task CanCheckAndUncheck() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = CreateNewScriptTaskList(sut, process, "Check And Uncheck");
        tasks.AddRange(await CreateGoToGutLookStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Check)));
        tasks.Add(sut.CreateVerifyWhetherEnabledTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), false));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Uncheck)));
        tasks.Add(sut.CreateVerifyWhetherEnabledTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), false));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.With)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "GutLookSubForm"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(IApplicationModel.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.WebViewCheckBoxesChecked), "NN"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Check)));
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), 2));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "CheckBox (CheckBox)"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(IApplicationModel.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.WebViewCheckBoxesChecked), "YN"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Uncheck)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "CheckBox (CheckBox)"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(IApplicationModel.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.WebViewCheckBoxesChecked), "NN"));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }
}