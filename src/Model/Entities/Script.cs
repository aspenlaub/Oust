using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;

public class Script : IGuid, ISetGuid {
    public const string NewScriptName = "New Script";

    [Key,XmlAttribute("guid")]
    public string Guid { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlElement("ScriptStep")]
    public ObservableCollection<ScriptStep> ScriptSteps { get; set; }

    public Script() {
        Guid = System.Guid.NewGuid().ToString();
        Name = "";
        ScriptSteps = new ObservableCollection<ScriptStep>();
    }

    public Script(Script script) : this() {
        Guid = script.Guid;
        Name = script.Name;
        script.ScriptSteps.OrderBy(scriptStep => scriptStep.StepNumber).ToList().ForEach(scriptStep => ScriptSteps.Add(scriptStep));
    }

    public List<ScriptStep> OrderedScriptSteps() {
        return ScriptSteps.OrderBy(s => s.StepNumber).ToList();
    }

    public Script Duplicate(string guid, string name) {
        var duplicate = new Script { Guid = guid, Name = name };
        ScriptSteps
            .OrderBy(scriptStep => scriptStep.StepNumber)
            .ToList()
            .ForEach(scriptStep => duplicate.ScriptSteps.Add(new ScriptStep(scriptStep) { StepNumber = scriptStep.StepNumber }));
        return duplicate;
    }
}