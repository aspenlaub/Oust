using System.Xml.Linq;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;

public class DumpFolderChecker : IDumpFolderChecker {
    private readonly EnvironmentType _EnvironmentType;
    private readonly IFolderResolver _FolderResolver;
    private readonly IContextFactory _ContextFactory;

    public DumpFolderChecker(EnvironmentType environmentType, IFolderResolver folderResolver, IContextFactory contextFactory) {
        _EnvironmentType = environmentType;
        _FolderResolver = folderResolver;
        _ContextFactory = contextFactory;
    }

    public async Task CheckDumpFolderAsync(IErrorsAndInfos errorsAndInfos) {
        var folder = (await _FolderResolver.ResolveAsync("$(MainUserFolder)", errorsAndInfos)).SubFolder("Oust").SubFolder(Enum.GetName(typeof(EnvironmentType), _EnvironmentType));
        if (errorsAndInfos.AnyErrors()) { return; }

        var dumpFolder = folder.SubFolder("Dump");
        dumpFolder.CreateIfNecessary();

        var guidToName = new Dictionary<string, string>();
        foreach (var fileName in Directory
                     .GetFiles(dumpFolder.FullName, "*.xml", SearchOption.AllDirectories)
                     .Where(fileName => fileName.Substring(dumpFolder.FullName.Length + 1).Contains('\\'))) {
            try {
                var document = XDocument.Load(fileName);
                var scriptElement = document.Root;
                var guid = scriptElement?.Attribute("guid")?.Value;
                if (string.IsNullOrWhiteSpace(guid)) {
                    errorsAndInfos.Errors.Add($"Could not read script dump {fileName}");
                    return;
                }
                var name = scriptElement.Attribute("name")?.Value;
                if (string.IsNullOrWhiteSpace(name)) {
                    errorsAndInfos.Errors.Add($"Could not read script dump {fileName}");
                    return;
                }

                if (guidToName.TryGetValue(guid, out var value)) {
                    errorsAndInfos.Errors.Add($"The same guid is assigned to dumped script {value} and {name}. Please delete the wrong and reimport the right script");
                    return;
                }

                guidToName[guid] = name;
            } catch {
                errorsAndInfos.Errors.Add($"Could not read script dump {fileName}");
            }
        }

        var context = await _ContextFactory.CreateAsync(_EnvironmentType);
        var scriptSteps = await context.ScriptSteps.Where(s => s.SubScriptName != "").ToListAsync();
        scriptSteps = scriptSteps.Where(s => guidToName.ContainsKey(s.SubScriptGuid) && guidToName[s.SubScriptGuid] != s.SubScriptName).ToList();
        if (scriptSteps.Any()) {
            scriptSteps.ForEach(s => s.SubScriptName = guidToName[s.SubScriptGuid]);
            await context.SaveChangesAsync();
        }
    }
}