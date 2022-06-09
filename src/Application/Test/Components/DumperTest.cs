using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Components;

[TestClass]
public class DumperTest {
    protected IDumper Sut;
    protected IDumperNameConverter DumperNameConverter;
    protected IContextFactory ContextFactory;
    private IContainer _Container;

    [TestInitialize]
    public async Task InitializeAsync() {
        _Container = (await new ContainerBuilder().RegisterForOustApplicationAsync()).Build();
        DumperNameConverter = _Container.Resolve<IDumperNameConverter>();
        ContextFactory = _Container.Resolve<IContextFactory>();
        Sut = new Dumper(EnvironmentType.UnitTest, _Container.Resolve<IFolderResolver>(), _Container.Resolve<IXmlSerializer>(), DumperNameConverter, ContextFactory);
        var generator = new TestDataGenerator(ContextFactory, _Container.Resolve<ILogicalUrlRepository>());
        await generator.GenerateTestDataAsync();
    }

    [TestCleanup]
    public async Task CleanUpAsync() {
        await TestFolderHelper.CleanUpAsync();
    }

    [TestMethod]
    public async Task CanDumpScript() {
        var dumpFolder = new Folder(await TestFolderHelper.TestFolderNameAsync(true));
        Assert.AreNotEqual("", dumpFolder.FullName, "Could not delete existing folders");
        Assert.IsTrue(await Sut.DumpScriptsAsync(dumpFolder, false), "Could not dump scripts to " + dumpFolder.FullName);

        var dumpSubFolder = dumpFolder.SubFolder(DumperNameConverter.DumpSubFolder(TestDataGenerator.Script1Name));
        var dumpFile = dumpSubFolder.FullName + '\\' + TestDataGenerator.Script1Name + ".xml";
        Assert.IsTrue(File.Exists(dumpFile), "Dump file 1 not found: " + dumpFile);
        var lines = await File.ReadAllLinesAsync(dumpFile);
        Assert.AreEqual(1, lines.Count(l => l.Contains("<Script ")));
        Assert.AreEqual(3, lines.Count(l => l.Contains("<ScriptStep ")));

        dumpSubFolder = dumpFolder.SubFolder(DumperNameConverter.DumpSubFolder(TestDataGenerator.Script2Name));
        dumpFile = dumpSubFolder.FullName + '\\' + TestDataGenerator.Script2Name + ".xml";
        Assert.IsTrue(File.Exists(dumpFile), "Dump file 2 not found: " + dumpFile);
        lines = await File.ReadAllLinesAsync(dumpFile);
        Assert.AreEqual(1, lines.Count(l => l.Contains("<Script ")));
        Assert.AreEqual(4, lines.Count(l => l.Contains("<ScriptStep ")));
    }

    [TestMethod]
    public async Task CommentInOldFileSurvivesDump() {
        var dumpFolder = new Folder(await TestFolderHelper.TestFolderNameAsync(true));
        Assert.AreNotEqual("", dumpFolder.FullName, "Could not delete existing folders");
        Assert.IsTrue(await Sut.DumpScriptsAsync(dumpFolder, false), "Could not dump scripts to " + dumpFolder.FullName);

        var dumpSubFolder = dumpFolder.SubFolder(DumperNameConverter.DumpSubFolder(TestDataGenerator.Script1Name));
        var dumpFile = dumpSubFolder.FullName + '\\' + TestDataGenerator.Script1Name + ".xml";
        Assert.IsTrue(File.Exists(dumpFile), "Dump file 1 not found: " + dumpFile);
        var lines = (await File.ReadAllLinesAsync(dumpFile)).ToList();
        lines.Insert(1, "<!-- V=100 -->");
        await File.WriteAllLinesAsync(dumpFile, lines);

        Assert.IsTrue(await Sut.DumpScriptsAsync(dumpFolder, false), "Could not dump scripts again");
        Assert.IsTrue(File.Exists(dumpFile), "Dump file 1 not found after dumping again: " + dumpFile);
        lines = (await File.ReadAllLinesAsync(dumpFile)).ToList();
        Assert.AreEqual("<!-- V=100 -->", lines[1]);
    }
}