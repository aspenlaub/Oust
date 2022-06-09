using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;

public class DumperNameConverter : IDumperNameConverter {
    public string ScriptFileFriendlyShortName(string scriptName) {
        return "oust_" + scriptName.ToLower().Replace(' ', '_').Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss");
    }

    public string DumpSubFolder(string scriptName) {
        scriptName = scriptName.Replace("Ä", "A").Replace("Ö", "O").Replace("Ü", "U");
        var s = "";
        for (var i = 0; s.Length < 4 && i < scriptName.Length; i++) {
            var c = scriptName[i];
            if (!char.IsLetter(c) || !char.IsUpper(c)) { continue; }

            s += c;
        }

        return s;
    }
}