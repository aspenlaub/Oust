using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.GUI;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Web;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

public class OustIntegrationTestBase {
    protected readonly IContainer Container;
    protected readonly ILogicalUrlRepository LogicalUrlRepository;

    public OustIntegrationTestBase() {
        Container = new ContainerBuilder().RegisterForOustIntegrationTest().Build();
        LogicalUrlRepository = Container.Resolve<ILogicalUrlRepository>();
    }

    protected async Task<OustWindowUnderTest> CreateOustWindowUnderTestAsync() {
        Assert.IsTrue(new HttpGate().IsLocalHostAvailableAsync().Result);
        var sut = Container.Resolve<OustWindowUnderTest>();
        await sut.InitializeAsync();
        var process = await sut.FindIdleProcessAsync();
        var tasks = new List<ControllableProcessTask> {
            sut.CreateVerifyIntegrationTestEnvironmentTask(process),
            sut.CreateResetTask(process)
        };
        await sut.RemotelyProcessTaskListAsync(process, tasks, false, (_, _) => Task.CompletedTask);
        return sut;
    }

    protected List<ControllableProcessTask> CreateNewScriptTaskList(OustWindowUnderTest sut, ControllableProcess process, string newScriptName) {
        return sut.CreateNewScriptTaskList(process, newScriptName);
    }

    protected async Task<List<ControllableProcessTask>> CreateGoToGutLookStepTaskListAsync(OustWindowUnderTest sut, ControllableProcess process) {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await LogicalUrlRepository.GetUrlAsync("GutLookForms", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return CreateGoToStepTaskList(sut, process, url);
    }

    protected async Task<List<ControllableProcessTask>> CreateGoToSodwatStepTaskListAsync(OustWindowUnderTest sut, ControllableProcess process) {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await LogicalUrlRepository.GetUrlAsync("SodWat", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return CreateGoToStepTaskList(sut, process, url);
    }

    protected async Task<List<ControllableProcessTask>> CreateGoToHockeyStepTaskListAsync(OustWindowUnderTest sut, ControllableProcess process) {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await LogicalUrlRepository.GetUrlAsync("Crawler", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return CreateGoToStepTaskList(sut, process, url);
    }

    protected async Task<List<ControllableProcessTask>> CreateGoToViperAdminStepTaskListAsync(OustWindowUnderTest sut, ControllableProcess process) {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await LogicalUrlRepository.GetUrlAsync("ViperAdmin", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return CreateGoToStepTaskList(sut, process, url);
    }

    protected List<ControllableProcessTask> CreateGoToStepTaskList(OustWindowUnderTest sut, ControllableProcess process, string url) {
        return new() {
            sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.GoToUrl)),
            sut.CreateSetValueTask(process, nameof(IApplicationModel.FreeText), url),
            sut.CreatePressButtonTask(process, nameof(OustWindow.AddOrReplaceStep))
        };
    }

    protected async Task StepIntoAsync(OustWindowUnderTest sut, ControllableProcess process, bool successIsExpected) {
        await sut.RemotelyPressButtonAsync(process, nameof(IApplicationModel.StepInto), successIsExpected);
    }

    protected List<ControllableProcessTask> CreateWithGutLookSubFormTaskList(OustWindowUnderTest sut, ControllableProcess process) {
        return new() {
            sut.CreateSetValueTask(process, nameof(IApplicationModel.ScriptStepType), Enum.GetName(typeof(ScriptStepType), ScriptStepType.With)),
            sut.CreateSetValueTask(process, nameof(IApplicationModel.FormOrControlOrIdOrClass), "GutLookSubForm")
        };
    }

    protected async Task<List<ControllableProcessTask>> CreateGoToInvalidMarkUpOnWrongCountTaskListAsync(OustWindowUnderTest sut, ControllableProcess process, int n) {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await LogicalUrlRepository.GetUrlAsync("InvalidMarkupOnWrongCount", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return CreateGoToStepTaskList(sut, process, url + "?n=" + n);
    }

    protected async Task<List<ControllableProcessTask>> CreateGoToErrorLogOnWrongCountTaskListAsync(OustWindowUnderTest sut, ControllableProcess process, int n) {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await LogicalUrlRepository.GetUrlAsync("ErrorLogOnWrongCount", errorsAndInfos);
        return CreateGoToStepTaskList(sut, process, url + "?n=" + n);
    }
}