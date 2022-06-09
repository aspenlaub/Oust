using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;

public class DenyWebClientValidationCriteriaSecret : ISecret<DenyWebClientValidationCriteria> {
    private DenyWebClientValidationCriteria _DenyWebClientValidationCriteria;
    public DenyWebClientValidationCriteria DefaultValue => _DenyWebClientValidationCriteria ??= new DenyWebClientValidationCriteria {
        new() { UrlPart = "/toughlook/" }
    };

    public string Guid => "D65516B4-6278-ED76-A4E2-D84CCA06271E";
}