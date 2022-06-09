using System.IO;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Test.Helpers;

public static class TestFolderHelper {
    private static readonly IContainer Container = new ContainerBuilder().UsePegh("Oust", new DummyCsArgumentPrompter()).Build();

    public static async Task<string> TestFolderNameAsync(bool empty) {
        var errorsAndInfos = new ErrorsAndInfos();
        var folder = await Container.Resolve<IFolderResolver>().ResolveAsync(@"$(GitHub)\Oust\src\Test\Temp", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        if (!empty) {
            return folder.FullName + '\\';
        }

        var deleter = new FolderDeleter();
        if (Directory.Exists(folder.FullName)) {
            if (!deleter.CanDeleteFolder(folder)) { return ""; }

            deleter.DeleteFolder(folder);
        }

        Directory.CreateDirectory(folder.FullName);
        var archiveFolder = await Container.Resolve<IFolderResolver>().ResolveAsync(@"$(GitHub)\Oust\src\Test\TempArchive", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        if (!Directory.Exists(archiveFolder.FullName)) {
            return folder.FullName + '\\';
        }

        deleter = new FolderDeleter();
        if (!deleter.CanDeleteFolder(archiveFolder)) { return ""; }

        deleter.DeleteFolder(archiveFolder);
        return folder.FullName + '\\';
    }

    public static async Task<string> ImportTestFolderNameAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var result = (await Container.Resolve<IFolderResolver>().ResolveAsync(@"$(GitHub)\Oust\src\Application\Test\ImportTest\", errorsAndInfos)).FullName + "\\";
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        return result;
    }

    public static async Task CleanUpAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var folder = await Container.Resolve<IFolderResolver>().ResolveAsync(@"$(GitHub)\Oust\src\Test", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        var deleter = new FolderDeleter();
        Assert.IsTrue(deleter.CanDeleteFolder(folder));
        deleter.DeleteFolder(folder);
    }
}