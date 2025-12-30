using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: DoNotParallelize]
namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Components;

[TestClass]
public class ImporterTest {
    protected IImporter Importer;
    protected IContextFactory ContextFactory;
    protected string Folder, InFolder, DoneFolder;
    private IContainer _Container;

    [TestInitialize]
    public async Task InitializeAsync() {
        _Container = (await new ContainerBuilder().RegisterForOustApplicationAsync(EnvironmentType.UnitTest)).Build();
        var logicalUrlRepository = _Container.Resolve<ILogicalUrlRepository>();
        Importer = new Importer(EnvironmentType.UnitTest, _Container.Resolve<IFolderResolver>(), _Container.Resolve<IXmlSerializer>(),
            _Container.Resolve<IXmlDeserializer>(), _Container.Resolve<IDumperNameConverter>(), _Container.Resolve<IContextFactory>(),
            logicalUrlRepository);
        ContextFactory = _Container.Resolve<IContextFactory>();
        var generator = new TestDataGenerator(ContextFactory, logicalUrlRepository);
        await generator.GenerateTestDataAsync();
        Folder = await TestFolderHelper.TestFolderNameAsync(true);
        Assert.AreNotEqual("", Folder, "Could not delete existing folders");
        InFolder = Folder + @"In\";
        Directory.CreateDirectory(InFolder);
        var importTestFolder = await TestFolderHelper.ImportTestFolderNameAsync() + @"Scripts\";
        var dirInfo = new DirectoryInfo(importTestFolder);
        dirInfo.GetFiles("*.xml").Select(x => x.Name).ToList().ForEach(x => File.Copy(importTestFolder + x, InFolder + x));
        DoneFolder = Folder + @"Done\";
        Directory.CreateDirectory(DoneFolder);
    }

    [TestCleanup]
    public async Task CleanUpAsync() {
        await TestFolderHelper.CleanUpAsync();
    }

    protected async Task<List<Script>> ScriptsInDatabaseAsync() {
        await using var context = SynchronizationContext.Current == null
         ? await ContextFactory.CreateAsync(EnvironmentType.UnitTest)
         : await ContextFactory.CreateAsync(EnvironmentType.UnitTest, SynchronizationContext.Current);

        return context.Scripts.Include(s => s.ScriptSteps).OrderBy(s => s.Name).ToList();
    }

    [TestMethod]
    public async Task CanImportScript() {
        Assert.IsTrue(File.Exists(InFolder + "Test Script 2.xml"));
        Assert.IsTrue(File.Exists(InFolder + "Test Script 3.xml"));
        var scripts = await ScriptsInDatabaseAsync();
        Assert.HasCount(3, scripts);
        Assert.AreEqual("Test Script 2", scripts[1].Name);
        Assert.Contains("unittest", scripts[1].OrderedScriptSteps()[0].Url);
        Assert.IsTrue(Importer.AnythingToImport(Folder));
        VerifyEndOfScript(scripts);
        var filesNamesToExclude = new List<string>();
        var importAFileResult = await Importer.ImportAFileAsync(InFolder, DoneFolder, filesNamesToExclude);
        Assert.IsTrue(importAFileResult.Success);
        Assert.IsEmpty(filesNamesToExclude);
        Assert.IsTrue(importAFileResult.CheckForMore);
        scripts = await ScriptsInDatabaseAsync();
        Assert.HasCount(3, scripts);
        Assert.AreEqual("Test Script 2", scripts[1].Name);
        Assert.DoesNotContain("unittest", scripts[1].OrderedScriptSteps()[0].Url);
        Assert.IsTrue(Importer.AnythingToImport(Folder));
        importAFileResult = await Importer.ImportAFileAsync(InFolder, DoneFolder, filesNamesToExclude);
        Assert.IsTrue(importAFileResult.Success);
        Assert.IsEmpty(filesNamesToExclude);
        Assert.IsTrue(importAFileResult.CheckForMore);
        VerifyEndOfScript(scripts);
        scripts = await ScriptsInDatabaseAsync();
        Assert.HasCount(4, scripts);
        Assert.IsFalse(Importer.AnythingToImport(Folder));
        Assert.HasCount(3, scripts[0].ScriptSteps);
        Assert.HasCount(4, scripts[1].ScriptSteps);
        Assert.AreEqual("Test Script 3", scripts[2].Name);
        Assert.HasCount(3, scripts[2].ScriptSteps);
    }

    private static void VerifyEndOfScript(IEnumerable<Script> scripts) {
        foreach (var steps in scripts.Select(script => script.OrderedScriptSteps())) {
            Assert.IsTrue(steps.Any());
            Assert.AreEqual(ScriptStepType.EndOfScript, steps[^1].ScriptStepType);
        }
    }
}