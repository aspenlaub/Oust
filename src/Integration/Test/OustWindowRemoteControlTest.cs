
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
public class OustWindowRemoteControlTest : OustIntegrationTestBase {
    [TestMethod]
    public async Task CanRemoteControlOust() {
        var generator = new TestDataGenerator(Container.Resolve<IContextFactory>(), LogicalUrlRepository);
        await generator.GenerateTestDataAsync();

        using var sut = await CreateOustWindowUnderTestAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = new List<ControllableProcessTask> {
            sut.CreateSetValueTask(process, nameof(IApplicationModel.SelectedScript), TestDataGenerator.Script1Name),
            sut.CreatePressButtonTask(process, nameof(OustWindow.CodeCoverage)),
            sut.CreatePressButtonTask(process, nameof(OustWindow.Play)),
            sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), "Script executed without errors"),
            sut.CreatePressButtonTask(process, nameof(OustWindow.StopCodeCoverage)),
            sut.CreateResetTask(process),
            sut.CreateVerifyValueTask(process, nameof(IApplicationModel.Status), "")
        };
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
    }
}