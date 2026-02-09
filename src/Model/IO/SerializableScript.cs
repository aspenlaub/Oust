using System.Text;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.IO;

[XmlRoot("Script", Namespace = "http://www.aspenlaub.net")]
public class SerializableScript : Script {
    private readonly IXmlSerializer _XmlSerializer;

    public SerializableScript() {
        IContainer container = new ContainerBuilder().UsePegh("Oust").Build();
        _XmlSerializer = container.Resolve<IXmlSerializer>();
    }

    public SerializableScript(IXmlSerializer xmlSerializer, Script script) : base(script) {
        _XmlSerializer = xmlSerializer;
    }

    public static Script ReadScriptFromFile(IXmlDeserializer xmlDeserializer, string folder, string fileName) {
        // ReSharper disable once StringLiteralTypo
        string text = File.ReadAllText(folder + fileName, Encoding.UTF8).Replace("steptype=\"Recognise\"", "steptype=\"Recognize\"");
        return xmlDeserializer.Deserialize<SerializableScript>(text);
    }

    public bool Save(string folder, string fileName, bool checkXsiXsd, string backupFolder) {
        string xml = _XmlSerializer.Serialize(this);
        if (string.IsNullOrEmpty(xml)) {
            return false;
        }

        if (File.Exists(folder + fileName)) {
            var lines = File.ReadAllLines(folder + fileName, Encoding.UTF8).ToList();
            if (lines.Any(string.IsNullOrWhiteSpace)) {
                lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                File.WriteAllLines(folder + fileName, lines, Encoding.UTF8);
            }
            lines = lines.Take(2).ToList();
            lines = lines.Where(l => l.StartsWith("<!--") && l.EndsWith("-->")).ToList();
            if (lines.Any()) {
                int pos = xml.IndexOf("\r\n", StringComparison.Ordinal);
                xml = xml.Substring(0, pos) + "\r\n" + string.Join("\r\n", lines) + xml.Substring(pos);
            }

            if (File.ReadAllText(folder + fileName, Encoding.UTF8) == xml) {
                return true;
            }

            for (int i = 0; i < 1000; i++) {
                string ending = "000" + i;
                string backupFileName = backupFolder + fileName.Substring(0, fileName.Length - 3) + ending.Substring(ending.Length - 3);
                if (File.Exists(backupFileName)) { continue; }

                File.Copy(folder + fileName, backupFileName);
                break;
            }
        }

        if (checkXsiXsd) {
            if (!xml.Contains("xmlns:xsi")) { return false; }
            if (!xml.Contains("xmlns:xsd")) { return false; }
            if (xml.IndexOf("xmlns:xsd", StringComparison.Ordinal) < xml.IndexOf("xmlns:xsi", StringComparison.Ordinal)) { return false; }
        }

        File.WriteAllText(folder + fileName, xml, Encoding.UTF8);
        return true;
    }
}