using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Components;

[TestClass]
public class DumperNameConverterTest {
    protected IDumperNameConverter Sut;

    [TestInitialize]
    public void Initialize() {
        Sut = new DumperNameConverter();
    }

    [TestMethod]
    public void CanProduceScriptFileNames() {
        Assert.AreEqual("oust_test_script_1", Sut.ScriptFileFriendlyShortName(TestDataGenerator.Script1Name));
        Assert.AreEqual("oust_test_script_2", Sut.ScriptFileFriendlyShortName(TestDataGenerator.Script2Name));
        Assert.AreEqual("oust_test_script_4", Sut.ScriptFileFriendlyShortName(TestDataGenerator.Script4Name));
    }

    [TestMethod]
    public void CanProduceDumpSubFolders() {
        Assert.AreEqual("TS", Sut.DumpSubFolder(TestDataGenerator.Script1Name));
        Assert.AreEqual("TS", Sut.DumpSubFolder(TestDataGenerator.Script2Name));
        Assert.AreEqual("TS", Sut.DumpSubFolder(TestDataGenerator.Script4Name));
        Assert.AreEqual("ANGH", Sut.DumpSubFolder(GetType().Namespace));
    }
}