using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;

public class ScriptCallResponse : ScriptCallResponseBase {
    public DomElement DomElement { get; set; } = new();
    public Dictionary<string, string> Dictionary { get; set; } = new();
    public string InnerHtml { get; set; } = "";
}