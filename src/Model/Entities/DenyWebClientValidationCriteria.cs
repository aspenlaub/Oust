using System.Collections.Generic;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;

[XmlRoot("DenyWebClientValidationCriteria", Namespace = "http://www.aspenlaub.net")]
public class DenyWebClientValidationCriteria : List<DenyWebClientValidationCriterion>, ISecretResult<DenyWebClientValidationCriteria> {
    public DenyWebClientValidationCriteria Clone() {
        var clone = new DenyWebClientValidationCriteria();
        clone.AddRange(this);
        return clone;
    }
}