using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;

public class FlawedTestDataGenerator {
    public const string FailingScriptGuid = "541944E0-34B5-4E71-8A7A-7A3BBEC7D571", FailingScriptName = "Script That Produces Invalid Html";
    public const string SubScriptGuid = "B27D6561-7257-497A-A280-E55AC054FEBD", SubScriptName = "Given This Is A Sub Script";
    public const string MasterScriptGuid = "446E36FA-CDBF-4BD8-BAF9-474C0F3E5492", MasterScriptName = "Master Script";
    public const string FailingScriptStepGuid = "1CB84525-82A9-48D3-8319-3401BF827E46";
    public const string MasterScriptStepGuid = "16A1123D-EAB3-4F7D-8493-4E65A1508E30";
    public const string SubScriptStepGuid = "87150AF3-EC68-4065-9C8B-AC80E575D5E8";

    private readonly IContextFactory _ContextFactory;
    private readonly ILogicalUrlRepository _LogicalUrlRepository;

    public FlawedTestDataGenerator(IContextFactory contextFactory, ILogicalUrlRepository logicalUrlRepository) {
        _ContextFactory = contextFactory;
        _LogicalUrlRepository = logicalUrlRepository;
    }

    protected async Task<Context> FreshContextAsync() {
        Assert.IsNotNull(SynchronizationContext.Current == null);

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
            var failingScript = new Script { Guid = FailingScriptGuid, Name = FailingScriptName };
            failingScript.ScriptSteps.Add(new ScriptStep { Guid = FailingScriptStepGuid, ScriptStepType = ScriptStepType.GoToUrl, StepNumber = 1, Url = await FailingScriptStepUrlAsync()});
            failingScript.ScriptSteps.Add(new ScriptStep { ScriptStepType = ScriptStepType.EndOfScript, StepNumber = 2 });
            context.Scripts.Add(failingScript);
            var subScript = new Script { Guid = SubScriptGuid, Name = SubScriptName };
            subScript.ScriptSteps.Add(new ScriptStep { Guid = SubScriptStepGuid, ScriptStepType = ScriptStepType.GoToUrl, StepNumber = 1, Url = await SubScriptStepUrlAsync() });
            subScript.ScriptSteps.Add(new ScriptStep { ScriptStepType = ScriptStepType.EndOfScript, StepNumber = 2 });
            context.Scripts.Add(subScript);
            var masterScript = new Script { Guid = MasterScriptGuid, Name = MasterScriptName };
            masterScript.ScriptSteps.Add(new ScriptStep {
                                             Guid = MasterScriptStepGuid,
                                             ScriptStepType = ScriptStepType.SubScript,
                                             StepNumber = 1,
                                             SubScriptGuid = SubScriptGuid,
                                             SubScriptName = SubScriptName
                                         });
            masterScript.ScriptSteps.Add(new ScriptStep { ScriptStepType = ScriptStepType.EndOfScript, StepNumber = 2 });
            context.Scripts.Add(masterScript);
            context.SaveChanges();
        }
    }

    public async Task<string> FailingScriptStepUrlAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await _LogicalUrlRepository.GetUrlAsync("InvalidMarkupOnWrongCount", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return url + "?n=4711";
    }

    public async Task<string> SubScriptStepUrlAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await _LogicalUrlRepository.GetUrlAsync("ViperAdmin", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return url;
    }
}