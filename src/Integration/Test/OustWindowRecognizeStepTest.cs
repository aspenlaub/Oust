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
        await sut.RemotelyProcessTaskListAsync(process, tasks);
    }

    [TestMethod]
    public async Task OutOfControlChoicesAreAvailableForAPageWithOucoForms() {
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        const string newName = "OutOfControl Choices Are Available For A Page With Ouco Forms";
        var tasks = CreateNewScriptTaskList(sut, process, newName);
        tasks.AddRange(await CreateGoToToughLookStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.With)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "ToughLookSubForm"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Recognize)));
        var expectedSelectableNames = new List<string> { "", "VerticalStack (Stack)", "HeaderAndTime (Copy)", "InputStack (Stack)", "InputLabel (Copy)",
            "Input (Input)", "Input (ValidationOutput)", "PasswordStack (Stack)", "PasswordLabel (Copy)", "Password (Input)", "Password (ValidationOutput)",
            "CheckBoxStack (Stack)", "CheckBox (CheckBox)", "CheckBoxLabel (Copy)", "CheckBox (ValidationOutput)", "DropDown (DropDown)",
            "DropDown (ValidationOutput)", "TextArea (TextArea)", "TextArea (ValidationOutput)", "RestrictedValueStack (Stack)", "RestrictedValueLabel (Copy)",
            "RestrictedValueInput (Restricted)", "RestrictedValueInput (ValidationOutput)", "Table (Table)", "SubFormButton (Button)" };
        tasks.Add(sut.CreateVerifyItemsTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), expectedSelectableNames));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
    }

    [TestMethod]
    public async Task CanRecognizeControl() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = CreateNewScriptTaskList(sut, process, "Recognize Control");
        tasks.AddRange(await CreateGoToToughLookStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.With)));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "ToughLookSubForm"));
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
        await sut.RemotelyProcessTaskListAsync(process, tasks);
        tasks.Clear();

        await sut.RemotelyPressButtonAsync(process, nameof(OustWindow.AddOrReplaceStep), false);

        const string errorMessage = "Control InputLabel (Copy) in instance 2 of ToughLookSubForm does not have the expected contents 'Input 1:'. Actual contents is 'Input 2:'";
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), errorMessage));
        tasks.AddRange(await CreateGoToSodwatStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptSteps), recognizeControlStepText));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
        tasks.Clear();

        await sut.RemotelyPressButtonAsync(process, nameof(OustWindow.StepInto), false);

        const string errorMessage2 = "No id or class context identified in preceding step/-s";
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), errorMessage2));
        await sut.RemotelyProcessTaskListAsync(process, tasks);
    }
}