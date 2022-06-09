namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IDumperNameConverter {
    string DumpSubFolder(string scriptName);
    string ScriptFileFriendlyShortName(string scriptName);
}