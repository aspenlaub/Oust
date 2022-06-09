using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.GUI;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

[TestClass]
public class OustWindowSubScriptTest : OustIntegrationTestBase {
    protected const string ScriptName = "Call Script", SubScriptName = "Sub Script One", AnotherSubScriptName = "Sub Script Two";
    protected const string FirstStepText = "Sub Script 'Sub Script One'";
    protected const string LastStepText = "Sub Script 'Sub Script Two'";
    protected string FirstSubStepText = "", LastSubStepText = "";

    [TestMethod]
    public async Task CanStepIntoAndOverSubScript() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = await CreateSubScriptsAndCallScriptAsync(sut, process);
        tasks.AddRange(StepIntoEverything(sut, process));
        tasks.AddRange(StepOverFirstSubScriptCall(sut, process));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
    }

    private async Task<List<ControllableProcessTask>> CreateSubScriptsAndCallScriptAsync(OustWindowUnderTest sut, ControllableProcess process) {
        var tasks = new List<ControllableProcessTask>();
        foreach (var subScriptName in new[] { SubScriptName, AnotherSubScriptName }) {
            tasks.AddRange(sut.CreateNewScriptTaskList(process, subScriptName));
            var addTasks = await CreateGoToToughLookStepTaskListAsync(sut, process);
            tasks.AddRange(addTasks);
            FirstSubStepText = "\u2192 " + addTasks[1].Text;
            tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), FirstSubStepText));
            addTasks = await CreateGoToHockeyStepTaskListAsync(sut, process);
            tasks.AddRange(addTasks);
            LastSubStepText = "\u2192 " + addTasks[1].Text;
            tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), LastSubStepText));
        }

        tasks.AddRange(sut.CreateNewScriptTaskList(process, ScriptName));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.SubScript)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.SubScript), SubScriptName));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), FirstStepText));

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.SubScript)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.SubScript), AnotherSubScriptName));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), LastStepText));

        return tasks;
    }

    private List<ControllableProcessTask> StepIntoEverything(WindowUnderTestActionsBase sut, ControllableProcess process) {
        var tasks = new List<ControllableProcessTask> {sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), FirstStepText)};
        for (var i = 0; i < 2; i++) {
            tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.StepInto)));
            tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), FirstSubStepText));
            tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.StepInto)));
            tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), LastSubStepText));
            tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.StepInto)));
            tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), Properties.Resources.EndOfScript));
            tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.StepInto)));
            tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps),
                i == 0 ? LastStepText : Properties.Resources.EndOfScript));
        }

        return tasks;
    }

    private List<ControllableProcessTask> StepOverFirstSubScriptCall(WindowUnderTestActionsBase sut, ControllableProcess process) {
        var tasks = new List<ControllableProcessTask> {
            sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), FirstStepText),
            sut.CreatePressButtonTask(process, nameof(OustWindow.StepOver)),
            sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), LastStepText)
        };

        return tasks;
    }
}