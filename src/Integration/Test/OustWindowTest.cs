using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.GUI;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

[TestClass]
public class OustWindowTest : OustIntegrationTestBase {

    [TestInitialize]
    public async Task Initialize() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
    }

    [TestMethod]
    public async Task CanOpenOust() {
        using (await CreateOustWindowUnderTestAsync()) { }
    }

    [TestMethod]
    public async Task SelectingAScriptChangesScriptSteps() {
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = new List<ControllableProcessTask> {
                sut.CreateSelectScriptTask(process, TestDataGenerator.Script2Name),
                sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.SelectedScript), 4),
                sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.ScriptSteps), 4)
            };
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }

    [TestMethod]
    public async Task RenameIsEnabledAndDisabledCorrectly() {
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = new List<ControllableProcessTask> {
                sut.CreateSelectScriptTask(process, TestDataGenerator.Script2Name),
                sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.SelectedScript), 4),
                sut.CreateVerifyWhetherEnabledTask(process, nameof(IApplicationModel.RenameScript), false),
                sut.CreateSetValueTask(process, nameof(IApplicationModel.NewScriptName), "Some Obscure Script Name"),
                sut.CreateVerifyWhetherEnabledTask(process, nameof(IApplicationModel.RenameScript), true),
                sut.CreateSetValueTask(process, nameof(IApplicationModel.NewScriptName), TestDataGenerator.Script2Name),
                sut.CreateVerifyWhetherEnabledTask(process, nameof(IApplicationModel.RenameScript), false),
                sut.CreateSetValueTask(process, nameof(IApplicationModel.NewScriptName), TestDataGenerator.Script4Name),
                sut.CreateVerifyWhetherEnabledTask(process, nameof(IApplicationModel.RenameScript), false),
            };
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }

    [TestMethod]
    public async Task CanRename() {
        using var sut = await CreateOustWindowUnderTestAsync();
        const string newName = "Some Obscure Script Name";
        var process = await sut.FindIdleProcessAsync();
        var tasks = new List<ControllableProcessTask> {
                sut.CreateSetValueTask(process, nameof(IApplicationModel.NewScriptName), newName),
                sut.CreatePressButtonTask(process, nameof(OustWindow.RenameScript)),
                sut.CreateVerifyValueTask(process, nameof(IApplicationModel.SelectedScript), newName)
            };
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }

}