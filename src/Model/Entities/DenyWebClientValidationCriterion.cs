using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;

public class DenyWebClientValidationCriterion {
    [XmlAttribute("urlpart")]
    public string UrlPart { get; set; }
}