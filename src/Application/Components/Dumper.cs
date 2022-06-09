using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;

public class Dumper : IDumper {
    private readonly IFolderResolver _FolderResolver;
    private readonly IXmlSerializer _XmlSerializer;
    private readonly EnvironmentType _EnvironmentType;
    private readonly IDumperNameConverter _DumperNameConverter;
    private readonly IContextFactory _ContextFactory;

    public Dumper(EnvironmentType environmentType, IFolderResolver folderResolver, IXmlSerializer xmlSerializer, IDumperNameConverter dumperNameConverter, IContextFactory contextFactory) {
        _EnvironmentType = environmentType;
        _FolderResolver = folderResolver;
        _XmlSerializer = xmlSerializer;
        _DumperNameConverter = dumperNameConverter;
        _ContextFactory = contextFactory;
    }

    public async Task<bool> DumpScriptsAsync(IFolder dumpFolder, bool checkXsiXsd) {
        var success = true;
        var backupFolder = dumpFolder.ParentFolder().SubFolder("Archive");
        backupFolder.CreateIfNecessary();
        await using var context = await _ContextFactory.CreateAsync(_EnvironmentType, SynchronizationContext.Current);
        foreach (var script in context.Scripts.Where(s => s.Name != Script.NewScriptName).Include(s => s.ScriptSteps)) {
            var serializableScript = new SerializableScript(_XmlSerializer, script);
            var dumpSubFolder = dumpFolder.SubFolder(_DumperNameConverter.DumpSubFolder(script.Name));
            dumpSubFolder.CreateIfNecessary();
            success = success && serializableScript.Save(dumpSubFolder.FullName + '\\', script.Name + ".xml", checkXsiXsd, backupFolder.FullName + '\\');
        }

        return success;
    }

    public async Task DumpAsync(IErrorsAndInfos errorsAndInfos) {
        var dumpFolder = (await _FolderResolver.ResolveAsync("$(MainUserFolder)", errorsAndInfos)).SubFolder("Oust").SubFolder(Enum.GetName(typeof(EnvironmentType), _EnvironmentType)).SubFolder("Dump");
        if (errorsAndInfos.AnyErrors()) { return; }

        dumpFolder.CreateIfNecessary();
        await DumpScriptsAsync(dumpFolder, true);
    }
}