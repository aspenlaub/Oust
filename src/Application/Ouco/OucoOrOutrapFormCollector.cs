using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Ouco;

public class OucoOrOutrapFormCollector {
    protected IOucoOrOutrapFormReader Reader;
    private readonly IFolderResolver _FolderResolver;

    private List<IOucoOrOutrapForm> _PrivateForms;

    public OucoOrOutrapFormCollector(IOucoOrOutrapFormReader reader, IFolderResolver folderResolver) {
        Reader = reader;
        _FolderResolver = folderResolver;
    }

    public async Task<List<IOucoOrOutrapForm>> GetFormsAsync() {
        if (_PrivateForms != null) { return _PrivateForms; }

        _PrivateForms = new List<IOucoOrOutrapForm>();
        var errorsAndInfos = new ErrorsAndInfos();
        var folder = (await _FolderResolver.ResolveAsync(@"$(WampRoot)", errorsAndInfos)).FullName;
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        InitializeFolder(folder);
        _PrivateForms = _PrivateForms.OrderBy(f => f.ToString()).ToList();
        return _PrivateForms;
    }

    private void InitializeFolder(string folder) {
        if (!Directory.Exists(folder)) { return; }
        if (IgnoreFolder(folder)) { return; }

        var dirInfo = new DirectoryInfo(folder);
        foreach (var form in from file in dirInfo.GetFiles("*.xml") select file.FullName into fileName where ContainsOucoFormTag(fileName) select Reader.Read(fileName, false)) {
            _PrivateForms.Add(form);
        }
        foreach (var form in from file in dirInfo.GetFiles("*.xml") select file.FullName into fileName where ContainsOutrapFormTag(fileName) select Reader.Read(fileName, true)) {
            _PrivateForms.Add(form);
        }
        foreach (var subDir in dirInfo.GetDirectories()) {
            InitializeFolder(subDir.FullName);
        }
    }

    private readonly List<string> _UnwantedSubFolders = new() { "css", "js", "temp", "upload" };

    private bool IgnoreFolder(string folder) {
        return _UnwantedSubFolders.Any(f => folder.EndsWith('\\' + f));
    }

    private static bool ContainsOucoFormTag(string fileName) {
        return File.ReadAllText(fileName).Contains("<form");
    }

    private static bool ContainsOutrapFormTag(string fileName) {
        return File.ReadAllText(fileName).Contains("<outrappage") || File.ReadAllText(fileName).Contains("<outrapform");
    }
}