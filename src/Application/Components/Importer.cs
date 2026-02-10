using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;

public class Importer : IImporter {
    private readonly EnvironmentType _EnvironmentType;
    private readonly IFolderResolver _FolderResolver;
    private readonly IXmlSerializer _XmlSerializer;
    private readonly IXmlDeserializer _XmlDeserializer;
    private readonly IDumperNameConverter _DumperNameConverter;
    private readonly IContextFactory _ContextFactory;
    private readonly ILogicalUrlRepository _LogicalUrlRepository;

    public Importer(EnvironmentType environmentType, IFolderResolver folderResolver, IXmlSerializer xmlSerializer, IXmlDeserializer xmlDeserializer,
        IDumperNameConverter dumperNameConverter, IContextFactory contextFactory, ILogicalUrlRepository logicalUrlRepository) {
        _DumperNameConverter = dumperNameConverter;
        _EnvironmentType = environmentType;
        _FolderResolver = folderResolver;
        _XmlSerializer = xmlSerializer;
        _XmlDeserializer = xmlDeserializer;
        _ContextFactory = contextFactory;
        _LogicalUrlRepository = logicalUrlRepository;
    }

    public bool AnythingToImport(string folder) {
        return FileList(InFolder(folder)).Any();
    }

    protected string InFolder(string folder) {
        return folder + @"In\";
    }

    protected IEnumerable<FileInfo> FileList(string inFolder) {
        var dirInfo = new DirectoryInfo(inFolder);
        return dirInfo.GetFiles("*.xml").OrderBy(f => f.Name);
    }

    public async Task<ImportAFileResult> ImportAFileAsync(string inFolder, string doneFolder, IList<string> fileNamesToExclude) {
        var importAFileResult = new ImportAFileResult  { Success = false, CheckForMore = false };
        var fileInfo = FileList(inFolder).FirstOrDefault(f => !fileNamesToExclude.Contains(f.FullName));
        if (fileInfo == null) { return importAFileResult; }

        importAFileResult.CheckForMore = true;
        SerializableScript deserializedScript;
        try {
            deserializedScript = new SerializableScript(_XmlSerializer, SerializableScript.ReadScriptFromFile(_XmlDeserializer, inFolder, fileInfo.Name));
        } catch {
            fileNamesToExclude.Add(fileInfo.FullName);
            return importAFileResult;
        }

        if (deserializedScript.Name == "") {
            fileNamesToExclude.Add(fileInfo.FullName);
            return importAFileResult;
        }

        await using (var context = await _ContextFactory.CreateAsync(_EnvironmentType, SynchronizationContext.Current)) {
            var existingScript = context.Scripts.Include(s => s.ScriptSteps).FirstOrDefault(s => s.Name == deserializedScript.Name || s.Guid.ToUpper() == deserializedScript.Guid.ToUpper());
            if (existingScript != null) {
                context.ScriptSteps.RemoveRange(existingScript.ScriptSteps);
                context.Scripts.Remove(existingScript);
            }
            var script = new Script(deserializedScript);
            ProperlyEndScript(script);
            await ReplaceLogicalUrlsAsync(script);
            context.Scripts.Add(script);
            try {
                context.SaveChanges();
            } catch {
                fileNamesToExclude.Add(fileInfo.FullName);
                return importAFileResult;
            }
        }

        if (File.Exists(doneFolder + fileInfo.Name)) {
            File.Delete(doneFolder + fileInfo.Name);
        }
        File.Move(fileInfo.FullName, doneFolder + fileInfo.Name);
        importAFileResult.Success = true;
        return importAFileResult;
    }

    private async Task ReplaceLogicalUrlsAsync(Script script) {
        foreach (var scriptStep in script.ScriptSteps) {
            if (scriptStep.ScriptStepType != ScriptStepType.GoToUrl && scriptStep.ScriptStepType != ScriptStepType.InvokeUrl) { continue; }
            if (scriptStep.Url.StartsWith("http")) { continue; }

            var errorsAndInfos = new ErrorsAndInfos();
            var url = await _LogicalUrlRepository.GetUrlAsync(scriptStep.Url, errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) { continue; }

            scriptStep.Url = url;
        }
    }

    private async Task CopyDumpFilesToInFolderWhenInNeedOfImportAsync(IFolder dumpFolder, IFolder inFolder, IFolder doneFolder) {
        await using var context = await _ContextFactory.CreateAsync(_EnvironmentType, SynchronizationContext.Current);
        var allScriptNames = await context.Scripts.Select(s => s.Name).ToListAsync();

        foreach (var fileName in Directory.GetFiles(dumpFolder.FullName, "*.xml", SearchOption.AllDirectories)) {
            var scriptFileShortName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            var doneFileName = doneFolder.FullName + '\\' + scriptFileShortName;
            if (File.Exists(doneFileName)
                && await File.ReadAllTextAsync(doneFileName, Encoding.UTF8) == await File.ReadAllTextAsync(fileName, Encoding.UTF8)
                && allScriptNames.Any(n => scriptFileShortName.StartsWith(n))) { continue; }

            var inFileName = inFolder.FullName + '\\' + scriptFileShortName;
            File.Copy(fileName, inFileName, true);
        }
    }

    private async Task DeleteScriptsNotFoundInDumpFolderAsync(IFolder dumpFolder) {
        await using var context = await _ContextFactory.CreateAsync(_EnvironmentType, SynchronizationContext.Current);
        var allScripts = await context.Scripts.Include(s => s.ScriptSteps).ToListAsync();
        var obsoleteScripts = allScripts.Where(s => IsObsoleteScriptAsync(s, dumpFolder, allScripts)).ToList();
        if (!obsoleteScripts.Any()) {
            return;
        }

        foreach (var obsoleteScript in obsoleteScripts) {
            context.ScriptSteps.RemoveRange(obsoleteScript.ScriptSteps);
            context.Scripts.Remove(obsoleteScript);
        }

        await context.SaveChangesAsync();
    }

    protected bool IsObsoleteScriptAsync(Script script, IFolder dumpFolder, List<Script> allScripts) {
        if (script.Name == Script.NewScriptName) { return false; }

        if (File.Exists(dumpFolder.FullName + '\\' + script.Name + ".xml")) { return false; }

        var dumpSubFolder = dumpFolder.SubFolder(_DumperNameConverter.DumpSubFolder(script.Name));
        if (File.Exists(dumpSubFolder.FullName + '\\' + script.Name + ".xml")) { return false; }

        var consumingScript = allScripts.FirstOrDefault(s => s.ScriptSteps.Any(stp => stp.SubScriptGuid == script.Guid && stp.SubScriptName == script.Name));
        return consumingScript == null;
    }

    protected void ProperlyEndScript(Script script) {
        var stepsToRemove = new List<ScriptStep>();
        for (var i = 0; i < script.ScriptSteps.Count - 1; i++) {
            if (script.ScriptSteps[i].ScriptStepType != ScriptStepType.EndOfScript) { continue; }

            stepsToRemove.Add(script.ScriptSteps[i]);
        }
        foreach (var scriptStep in stepsToRemove) {
            script.ScriptSteps.Remove(scriptStep);
        }

        if (script.ScriptSteps.Count == 0 || script.ScriptSteps[^1].ScriptStepType != ScriptStepType.EndOfScript) {
            script.ScriptSteps.Add(new ScriptStep { ScriptStepType = ScriptStepType.EndOfScript, StepNumber = script.ScriptSteps.Count == 0 ? 1 : script.ScriptSteps.Max(s => s.StepNumber + 1) });
        }
    }

    public async Task ImportAsync(IErrorsAndInfos errorsAndInfos) {
        var folder = (await _FolderResolver.ResolveAsync("$(MainUserFolder)", errorsAndInfos)).SubFolder("Oust").SubFolder(Enum.GetName(typeof(EnvironmentType), _EnvironmentType));
        if (errorsAndInfos.AnyErrors()) { return; }

        var dumpFolder = folder.SubFolder("Dump");
        dumpFolder.CreateIfNecessary();
        var inFolder = folder.SubFolder("In");
        inFolder.CreateIfNecessary();
        var doneFolder = folder.SubFolder("Done");
        doneFolder.CreateIfNecessary();
        if (_EnvironmentType != EnvironmentType.UnitTest) {
            await DeleteScriptsNotFoundInDumpFolderAsync(dumpFolder);
            await CopyDumpFilesToInFolderWhenInNeedOfImportAsync(dumpFolder, inFolder, doneFolder);
        }
        var filesNamesToExclude = new List<string>();
        ImportAFileResult importAFileResult;
        do {
            importAFileResult = await ImportAFileAsync(inFolder.FullName + '\\', doneFolder.FullName + '\\', filesNamesToExclude);
        } while (importAFileResult.CheckForMore);

        var numberOfFiles = Directory.GetFiles(dumpFolder.FullName, "*.xml").ToList().Count;
        await using var context = await _ContextFactory.CreateAsync(_EnvironmentType, SynchronizationContext.Current);
        var numberOfScripts = await context.Scripts.CountAsync(s => !string.IsNullOrWhiteSpace(s.Name));
        if (numberOfScripts >= numberOfFiles) {
            return;
        }

        errorsAndInfos.Errors.Add($"Number of dump files is {numberOfFiles}, {numberOfScripts} exist in the database");
    }
}