using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;

public class OustSetting {
    [XmlAttribute("machine")]
    public string Machine { get; set; }

    [XmlAttribute("shouldwindows11beassumed")]
    public bool ShouldWindows11BeAssumed { get; set; }
}