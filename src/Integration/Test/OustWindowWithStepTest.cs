using System;
using System.Collections.Generic;
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
public class OustWindowWithStepTest : OustIntegrationTestBase {
    [TestInitialize]
    public async Task Initialize() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
    }

    [TestMethod]
    public async Task FormOrControlIdOrClassForWithStepIsLabeledForm() {
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = new List<ControllableProcessTask> {
            sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.With)),
            sut.CreateVerifyLabelTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), Properties.Resources.Form)
        };
        await sut.RemotelyProcessTaskListAsync(process, tasks);
    }

    [TestMethod]
    public async Task FormChoicesAreAvailableForAPageWithOucoForms() {
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = sut.CreateNewScriptTaskList(process, "Form Choices Are Available For A Page With Ouco Forms");
        tasks.AddRange(await CreateGoToToughLookStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.With)));
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), 4));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), ""));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "ToughLookForm"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "ToughLookFrame"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "ToughLookSubForm"));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
    }

    [TestMethod]
    public async Task CanRecognizeForm() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = sut.CreateNewScriptTaskList(process, "Recognize Form");
        var addTasks = await CreateGoToToughLookStepTaskListAsync(sut, process);
        tasks.AddRange(addTasks);
        var goToToughLookStepText = "\u2192 " + addTasks[1].Text;
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.With)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "ToughLookSubForm"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
        tasks.Clear();

        await sut.RemotelySetValueAsync(process, nameof(IApplicationModel.FormOrIdOrClassInstanceNumber), "3");

        tasks.Add(sut.CreateVerifyWhetherEnabledTask(process, nameof(IApplicationModel.AddOrReplaceStep), false));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrIdOrClassInstanceNumber), "2"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        const string withFormStepText = "With second ToughLookSubForm";
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), withFormStepText));
        addTasks = await CreateGoToSodwatStepTaskListAsync(sut, process);
        tasks.AddRange(addTasks);
        var goToSodwatStepText = "\u2192 " + addTasks[1].Text;
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToToughLookStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.StepInto)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), withFormStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.StepInto)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), goToSodwatStepText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.StepInto)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), withFormStepText));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
        tasks.Clear();

        await sut.RemotelyPressButtonAsync(process, nameof(OustWindow.StepInto), false);

        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), "Instance 2 of ToughLookSubForm not found"));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
    }
}