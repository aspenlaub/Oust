using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Components;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;

[TestClass]
public class TestDataGenerator {
    public const string Script1Guid = "FB50F71C-54E3-E6B1-3E7F-92B8220323F5", Script1Name = "Test Script 1";
    public const string Script2Guid = "BFDCF6C4-6384-2210-188C-A8A29DB8FEB9", Script2Name = "Test Script 2";
    public const string Script4Guid = "B5066AC8-1C45-5F28-3E33-5907F3413363", Script4Name = "Test Script 4";
    public const string ScriptStep11Guid = "FD70BD87-99E8-43DF-7555-6765A72B3BAC";
    public const string ScriptStep12Guid = "A6D7DF80-FAFA-BAEE-C78F-B25289699AD0";
    public const string ScriptStep21Guid = "D09E2AF6-C96E-9753-464A-3EB268373D1A";
    public const string ScriptStep22Guid = "FA12963C-9365-2727-0D7F-411A66288763";
    public const string ScriptStep23Guid = "FCAD8E95-BE7E-4801-E7E2-27FB14CD6E44";
    public const string ScriptStep41Guid = "EBE89822-4E43-E27C-C3C1-0E80E04D615F";

    private readonly IContextFactory _ContextFactory;
    private readonly ILogicalUrlRepository _LogicalUrlRepository;

    public TestDataGenerator() : this (new ContextFactory(), CreateLogicalUrlRepository()) {
    }

    private static ILogicalUrlRepository CreateLogicalUrlRepository() {
        var container = new ContainerBuilder().UsePegh("Oust", new DummyCsArgumentPrompter()).Build();
        return new LogicalUrlRepository(container.Resolve<ISecretRepository>());
    }

    public TestDataGenerator(IContextFactory contextFactory, ILogicalUrlRepository logicalUrlRepository) {
        _ContextFactory = contextFactory;
        _LogicalUrlRepository = logicalUrlRepository;
    }

    protected async Task<Context> FreshContextAsync() {
        if (SynchronizationContext.Current == null) {
            return await _ContextFactory.CreateAsync(EnvironmentType.UnitTest);
        }

        return await _ContextFactory.CreateAsync(EnvironmentType.UnitTest, SynchronizationContext.Current);
    }

    public async Task GenerateTestDataAsync() {
        await using (var context = await FreshContextAsync()) {
            context.Migrate();
            context.ScriptSteps.RemoveRange(context.ScriptSteps);
            context.Scripts.RemoveRange(context.Scripts);
            context.SaveChanges();
        }

        await using (var context = await FreshContextAsync()) {
            var script1 = new Script { Guid = Script1Guid, Name = Script1Name };
            script1.ScriptSteps.Add(new ScriptStep { Guid = ScriptStep11Guid, ScriptStepType = ScriptStepType.GoToUrl, StepNumber = 1, Url = await ScriptStep11UrlAsync() });
            script1.ScriptSteps.Add(new ScriptStep { Guid = ScriptStep12Guid, ScriptStepType = ScriptStepType.GoToUrl, StepNumber = 2, Url = await ScriptStep12UrlAsync() });
            script1.ScriptSteps.Add(new ScriptStep { ScriptStepType = ScriptStepType.EndOfScript, StepNumber = 3 });
            context.Scripts.Add(script1);
            var script2 = new Script { Guid = Script2Guid, Name = Script2Name };
            script2.ScriptSteps.Add(new ScriptStep { Guid = ScriptStep21Guid, ScriptStepType = ScriptStepType.GoToUrl, StepNumber = 1, Url = await ScriptStep21UrlAsync() });
            script2.ScriptSteps.Add(new ScriptStep { Guid = ScriptStep22Guid, ScriptStepType = ScriptStepType.GoToUrl, StepNumber = 2, Url = await ScriptStep22UrlAsync() });
            script2.ScriptSteps.Add(new ScriptStep { Guid = ScriptStep23Guid, ScriptStepType = ScriptStepType.GoToUrl, StepNumber = 3, Url = await ScriptStep23UrlAsync() });
            script2.ScriptSteps.Add(new ScriptStep { ScriptStepType = ScriptStepType.EndOfScript, StepNumber = 4 });
            context.Scripts.Add(script2);
            var script4 = new Script { Guid = Script4Guid, Name = Script4Name };
            script4.ScriptSteps.Add(new ScriptStep { Guid = ScriptStep41Guid, ScriptStepType = ScriptStepType.GoToUrl, StepNumber = 1, Url = await ScriptStep41UrlAsync() });
            script4.ScriptSteps.Add(new ScriptStep { ScriptStepType = ScriptStepType.EndOfScript, StepNumber = 2 });
            context.Scripts.Add(script4);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task CanGenerateTestData() {
        await GenerateTestDataAsync();
        await using var context = await FreshContextAsync();
        var scripts = context.Scripts.OrderBy(s => s.Name).Include(s => s.ScriptSteps).ToList();
        Assert.AreEqual(3, scripts.Count);
        var script = scripts[0];
        var scriptSteps = script.OrderedScriptSteps();
        Assert.AreEqual(Script1Guid, script.Guid);
        Assert.AreEqual(3, scriptSteps.Count);
        Assert.AreEqual(ScriptStep11Guid, scriptSteps[0].Guid);
        Assert.AreEqual(ScriptStep12Guid, scriptSteps[1].Guid);
        script = scripts[1];
        scriptSteps = script.OrderedScriptSteps();
        Assert.AreEqual(Script2Name, script.Name);
        Assert.AreEqual(4, scriptSteps.Count);
        Assert.AreEqual(1, scriptSteps[0].StepNumber);
        Assert.AreEqual(await ScriptStep22UrlAsync(), scriptSteps[1].Url);
        script = scripts[2];
        scriptSteps = script.OrderedScriptSteps();
        Assert.AreEqual(Script4Name, script.Name);
        Assert.AreEqual(2, scriptSteps.Count);
        Assert.AreEqual(await ScriptStep41UrlAsync(), scriptSteps[0].Url);
    }

    public async Task<string> ScriptStep11UrlAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await _LogicalUrlRepository.GetUrlAsync("ViperAdmin", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return url;
    }

    public async Task<string> ScriptStep12UrlAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await _LogicalUrlRepository.GetUrlAsync("SodWat", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return url;
    }

    public async Task<string> ScriptStep21UrlAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await _LogicalUrlRepository.GetUrlAsync("ViperUnitTest", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return url;
    }

    public async Task<string> ScriptStep22UrlAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await _LogicalUrlRepository.GetUrlAsync("Herzblut", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return url;
    }

    public async Task<string> ScriptStep23UrlAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await _LogicalUrlRepository.GetUrlAsync("Viperfisch", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return url;
    }

    public async Task<string> ScriptStep41UrlAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await _LogicalUrlRepository.GetUrlAsync("GutLookForms", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return url;
    }
}