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
public class OustWindowInputStepTest : OustIntegrationTestBase {
    [TestMethod]
    public async Task CanInput() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();
        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = CreateNewScriptTaskList(sut, process, "Input Some Text");
        tasks.AddRange(await CreateGoToGutLookStepTaskListAsync(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Input)));
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), 0));
        tasks.AddRange(CreateWithGutLookSubFormTaskList(sut, process));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrIdOrClassInstanceNumber), "2"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.ScriptSteps), 2));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Input)));
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), 8));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "Input (Input)"));
        const string inputText = ":2 tupnI";
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FreeText), inputText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.ScriptSteps), 3));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        await sut.RemotelyProcessTaskListAsync(process, tasks, true, (_, _) => Task.CompletedTask);
        tasks.Clear();

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "RestrictedValueInput (Restricted)"));
        const string restrictedInputText = "Interläken";
        const string restrictedInputCorrectedText = "Interlaken";
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FreeText), restrictedInputText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.ScriptSteps), 4));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        await sut.RemotelyProcessTaskListAsync(process, tasks, true, (_, _) => Task.CompletedTask);
        tasks.Clear();

        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "TextArea (TextArea)"));
        const string textBoxText = @"public void DoNothing() {\n}";
        var textBoxConvertedText = textBoxText.Replace(@"\n", "\n");
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FreeText), textBoxText));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.ScriptSteps), 5));
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), ""));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.Press)));
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), 2));
        tasks.Add(sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "SubButton (Button)"));
        tasks.Add(sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep)));
        tasks.Add(sut.CreateVerifyNumberOfItemsTask(process, nameof(IApplicationModel.ScriptSteps), 6));
        var expectedInputValues = "2\t0\t0\t\t\tInterimscoach\t2\t0\t1\t" + inputText + "\t\t" + restrictedInputCorrectedText + "\t\t\t" + textBoxConvertedText;
        tasks.Add(sut.CreateVerifyValueTask(process, nameof(IApplicationModel.WebViewInputValues), expectedInputValues));
        await sut.RemotelyProcessTaskListAsync(process, tasks, true, OnTaskCompleted);
    }

    protected async Task OnTaskCompleted(int i, ControllableProcessTask _) {
        Assert.IsTrue(i >= 0);
        await Task.CompletedTask;
    }
}