using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.GUI;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

[TestClass]
public class OustWindowSelectStepTest : OustIntegrationTestBase {
    [TestMethod]
    public async Task CanSelect() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = CreateNewScriptTaskList(sut, process, "Make A Selection");
        tasks.AddRange(await CreateGoToGutLookStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Select)));
        tasks.Add(sut.CreateVerifyWhetherEnabledTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), false));
        tasks.AddRange(CreateWithGutLookSubFormTaskList(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrIdOrClassInstanceNumber), "2"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.WebViewSelectedValues), "Zwölf\tZwölf"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Select)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "DropDown (DropDown)"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.SelectedValue), "Vierundzwanzig"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.WebViewSelectedValues), "Zwölf\tVierundzwanzig"));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, async (i, task) => await OnTaskCompleted(i, task));
    }

    protected async Task OnTaskCompleted(int i, ControllableProcessTask _) {
        Assert.IsGreaterThanOrEqualTo(0, i);
        await Task.CompletedTask;
    }
}