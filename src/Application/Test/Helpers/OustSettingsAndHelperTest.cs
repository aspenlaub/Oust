using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;

[TestClass]
public class OustSettingsAndHelperTest {
    [TestMethod]
    public async Task CanGetOustSettingsUsingHelper() {
        IContainer container = new ContainerBuilder().UseDvinAndPegh("Oust").Build();
        var helper = new OustSettingsHelper(container.Resolve<ISecretRepository>());
        var errorsAndInfos = new ErrorsAndInfos();
        YesNoInconclusive shouldWindows11BeAssumed = await helper.ShouldWindows11BeAssumedAsync(errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        Assert.IsFalse(shouldWindows11BeAssumed.Inconclusive);
        if (Environment.OSVersion.Version.Build >= 22000) {
            Assert.IsTrue(shouldWindows11BeAssumed.YesNo, "This is Windows 11, so the setting should be true");
        }
    }
}