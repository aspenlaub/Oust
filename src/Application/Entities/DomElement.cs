using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;

public class DomElement {
    public DomElement AncestorDomElement { get; set; } = null;

    public YesNoInconclusive IsDiv { get; set; } = new();
    public YesNoInconclusive IsAnchor { get; set; } = new();
    public YesNoInconclusive IsInput { get; set; } = new();
    public YesNoInconclusive IsTextArea { get; set; } = new();

    public string Id { get; set; }
    public List<string> Classes { get; set; }

    public NthOfClass NthOfClass { get; set; } = null;
}