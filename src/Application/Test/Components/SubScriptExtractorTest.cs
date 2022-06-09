using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Components;

[TestClass]
public class SubScriptExtractorTest {
    private IContextFactory _ContextFactory;

    [TestInitialize]
    public void Initialize() {
        _ContextFactory = new ContextFactory();
    }

    [TestMethod]
    public async Task CanExtractSubScript() {
        var container = (await new ContainerBuilder().UseVishizhukelNetWebAndPeghAsync("Oust", new DummyCsArgumentPrompter())).Build();
        var generator = new TestDataGenerator(_ContextFactory, container.Resolve<ILogicalUrlRepository>());
        await generator.GenerateTestDataAsync();
        var orderedScriptSteps = await WhenTwoScriptStepsAreExtracted();
        await ThenTheStepsAreReplacedInTheScriptAndTheNewSubScriptContainsThemAsync(orderedScriptSteps);
    }

    private static async Task<List<ScriptStep>> WhenTwoScriptStepsAreExtracted() {
        var contextFactory = new ContextFactory();
        var context = await contextFactory.CreateAsync(EnvironmentType.UnitTest);
        var script = context.Scripts.Include(s => s.ScriptSteps).SingleOrDefault(s => s.Guid == TestDataGenerator.Script2Guid);
        Assert.IsNotNull(script);
        var orderedScriptSteps = script.OrderedScriptSteps();
        Assert.AreEqual(4, orderedScriptSteps.Count);
        var sut = new SubScriptExtractor(contextFactory);
        const string subScriptNameTemplate = $"Sub script of {TestDataGenerator.Script2Name}";
        var extractSubScriptSpecification = new ExtractSubScriptSpecification(subScriptNameTemplate, new List<Selectable> {
            new() {Guid = orderedScriptSteps[1].Guid, Name = orderedScriptSteps[1].SubScriptName},
            new() {Guid = orderedScriptSteps[2].Guid, Name = orderedScriptSteps[2].SubScriptName},
        });
        await sut.ExtractSubScriptAsync(EnvironmentType.UnitTest, TestDataGenerator.Script2Guid, extractSubScriptSpecification);
        return orderedScriptSteps;
    }

    private async Task ThenTheStepsAreReplacedInTheScriptAndTheNewSubScriptContainsThemAsync(IReadOnlyList<ScriptStep> orderedScriptSteps) {
        var context = await _ContextFactory.CreateAsync(EnvironmentType.UnitTest);
        var script = context.Scripts.Include(s => s.ScriptSteps).SingleOrDefault(s => s.Guid == TestDataGenerator.Script2Guid);
        Assert.IsNotNull(script);
        var newOrderedScriptSteps = script.OrderedScriptSteps();
        Assert.AreEqual(3, newOrderedScriptSteps.Count);
        Assert.AreEqual(orderedScriptSteps[0].Guid, newOrderedScriptSteps[0].Guid);
        Assert.AreEqual(ScriptStepType.SubScript, newOrderedScriptSteps[1].ScriptStepType);
        Assert.AreEqual(orderedScriptSteps[3].Guid, newOrderedScriptSteps[2].Guid);
        var subScript = context.Scripts.Include(s => s.ScriptSteps).SingleOrDefault(s => s.Guid == newOrderedScriptSteps[1].SubScriptGuid);
        Assert.IsNotNull(subScript);
        var orderedSubScriptSteps = subScript.OrderedScriptSteps();
        Assert.AreEqual(3, orderedSubScriptSteps.Count);
        Assert.AreEqual(orderedScriptSteps[1].ToString(), orderedSubScriptSteps[0].ToString());
        Assert.AreEqual(orderedScriptSteps[2].ToString(), orderedSubScriptSteps[1].ToString());
        Assert.AreNotEqual(orderedScriptSteps[1].ToString(), orderedSubScriptSteps[0].Guid);
        Assert.AreNotEqual(orderedScriptSteps[2].Guid, orderedSubScriptSteps[1].Guid);
        Assert.AreEqual(ScriptStepType.EndOfScript, orderedSubScriptSteps[2].ScriptStepType);
    }
}