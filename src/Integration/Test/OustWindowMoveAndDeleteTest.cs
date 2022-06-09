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
public class OustWindowMoveAndDeleteTest : OustIntegrationTestBase {
    [TestInitialize]
    public async Task Initialize() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
    }

    [TestMethod]
    public async Task CanMoveAndDeleteScriptSteps() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = CreateNewScriptTaskList(sut, process, "Move And Delete Script Steps");
        var goToToughLookTasks = await CreateGoToToughLookStepTaskListAsync(sut, process);
        tasks.AddRange(goToToughLookTasks);
        var goToToughLookStepText = "\u2192 " + goToToughLookTasks[1].Text;
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), goToToughLookStepText));
        var goToSodwatTasks = await CreateGoToSodwatStepTaskListAsync(sut, process);
        tasks.AddRange(goToSodwatTasks);
        var goToSodwatStepText = "\u2192 " + goToSodwatTasks[1].Text;
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), goToSodwatStepText));
        var goToHockeyTasks = await CreateGoToHockeyStepTaskListAsync(sut, process);
        tasks.AddRange(goToHockeyTasks);
        var goToHockeyStepText = "\u2192 " + goToHockeyTasks[1].Text;
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), goToHockeyStepText));
        var goToAdminTasks = await CreateGoToViperAdminStepTaskListAsync(sut, process);
        tasks.AddRange(goToAdminTasks);
        var goToViperAdminStepText = "\u2192 " + goToAdminTasks[1].Text;
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), goToViperAdminStepText));
        AddTasksToCheckOrderOfSteps(sut, process, tasks, new List<string> { goToToughLookStepText, goToSodwatStepText, goToHockeyStepText, goToViperAdminStepText });

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToHockeyStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.MoveUp)));
        AddTasksToCheckOrderOfSteps(sut, process, tasks, new List<string> { goToToughLookStepText, goToHockeyStepText, goToSodwatStepText, goToViperAdminStepText });

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToViperAdminStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.MoveUp)));
        AddTasksToCheckOrderOfSteps(sut, process, tasks, new List<string> { goToToughLookStepText, goToHockeyStepText, goToViperAdminStepText, goToSodwatStepText });

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToSodwatStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.MoveUp)));
        AddTasksToCheckOrderOfSteps(sut, process, tasks, new List<string> { goToToughLookStepText, goToHockeyStepText, goToSodwatStepText, goToViperAdminStepText });

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToHockeyStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.MoveUp)));
        AddTasksToCheckOrderOfSteps(sut, process, tasks, new List<string> { goToHockeyStepText, goToToughLookStepText, goToSodwatStepText, goToViperAdminStepText });

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToSodwatStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.DeleteStep)));
        AddTasksToCheckOrderOfSteps(sut, process, tasks, new List<string> { goToHockeyStepText, goToToughLookStepText, goToViperAdminStepText });

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToToughLookStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.DeleteStep)));
        AddTasksToCheckOrderOfSteps(sut, process, tasks, new List<string> { goToHockeyStepText, goToViperAdminStepText });

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToHockeyStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.DeleteStep)));
        AddTasksToCheckOrderOfSteps(sut, process, tasks, new List<string> { goToViperAdminStepText });

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToViperAdminStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.DeleteStep)));
        AddTasksToCheckOrderOfSteps(sut, process, tasks, new List<string>());

        await sut.RemotelyProcessTaskListAsync(process, tasks);
    }

    protected void AddTasksToCheckOrderOfSteps(OustWindowUnderTest sut, ControllableProcess process, List<ControllableProcessTask> tasks, IList<string> expectedOrder) {
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.ScriptSteps), expectedOrder.Count));
        tasks.Add(sut.CreateVerifyItemsTask(process, nameof(IApplicationModel.ScriptSteps), expectedOrder));
    }
}