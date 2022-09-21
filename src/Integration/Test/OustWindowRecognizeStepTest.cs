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
public class OustWindowRecognizeStepTest : OustIntegrationTestBase {
    [TestInitialize]
    public async Task Initialize() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
    }

    [TestMethod]
    public async Task FreeTextForRecognizeStepIsLabeledExpectedContents() {
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = new List<ControllableProcessTask> {
            sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Recognize)),
            sut.CreateVerifyLabelTask(process, nameof(IApplicationModel.FreeText), Properties.Resources.FreeTextExpectedContents)
        };
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }

    [TestMethod]
    public async Task OutOfControlChoicesAreAvailableForAPageWithOutrapForms() {
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        const string newName = "OutOfControl Choices Are Available For A Page With Outrap Forms";
        var tasks = CreateNewScriptTaskList(sut, process, newName);
        tasks.AddRange(await CreateGoToGutLookStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.With)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "GutLookSubForm"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Recognize)));
        var expectedSelectableNames = new List<string> { "", "ImageFrame (Frame)", "Image (Image)", "HelloRow (Row)", "HelloColumn (Column)", "HelloWorld (Copy)",
            "HelloFreeWorld (Copy)", "Microtime (Copy)", "VariousInputRow (Row)", "CounterWithinLabel (Copy)", "CounterWithinInput (Input)", "CounterOutsideLabel (Copy)",
            "CounterOutsideInput (Input)", "CounterPressLabel (Copy)", "CounterPressInput (Input)", "InputLabel (Copy)", "InputColumn (Column)", "Input (Input)",
            "PasswordLabel (Copy)", "Password (Input)", "CheckBoxCol (Column)", "CheckBox (CheckBox)", "CheckBoxLabel (Copy)", "TextAreaLabel (Copy)",
            "TextArea (TextArea)", "DropDownLabel (Copy)", "DropDown (DropDown)", "RestrictedValueLabel (Copy)", "RestrictedValueInput (Restricted)",
            "RestrictedValueInput (ValidationOutput)", "RowsToBeAdded (Frame)", "SubButton (Button)"
        };
        tasks.Add(sut.CreateVerifyItemsTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), expectedSelectableNames));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }

    [TestMethod]
    public async Task CanRecognizeControl() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = CreateNewScriptTaskList(sut, process, "Recognize Control");
        tasks.AddRange(await CreateGoToGutLookStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.With)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "GutLookSubForm"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrIdOrClassInstanceNumber), "2"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Recognize)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "InputLabel (Copy)"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ExpectedContents), "Input 2:"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        const string recognizeControlStepText = "Recognize InputLabel (Copy) with contents 'Input 2:'";
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.ScriptSteps), recognizeControlStepText));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Recognize)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "InputLabel (Copy)"));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ExpectedContents), "Input 1:"));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, async (i, task) => await OnTaskCompleted(i, task));
        tasks.Clear();

        await sut.RemotelyPressButtonAsync(process, nameof(OustWindow.AddOrReplaceStep), false);

        const string errorMessage = "Control InputLabel (Copy) in instance 2 of GutLookSubForm does not have the expected contents 'Input 1:'. Actual contents is 'Input 2:'";
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), errorMessage));
        tasks.AddRange(await CreateGoToSodwatStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), recognizeControlStepText));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
        tasks.Clear();

        await sut.RemotelyPressButtonAsync(process, nameof(OustWindow.StepInto), false);

        const string errorMessage2 = "No id or class context identified in preceding step/-s";
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), errorMessage2));
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }

    protected async Task OnTaskCompleted(int i, ControllableProcessTask _) {
        Assert.IsTrue(i >= 0);
        await Task.CompletedTask;
    }
}