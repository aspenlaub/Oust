using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IOustScriptStatementFactory {
    IScriptStatement CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(string idOrClass, int n);
    IScriptStatement CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(string idOrClass, int n, string name);
    IScriptStatement CreateDoesDocumentContainDescendantElementOfClassStatement(string ancestorDomElementJson, string className, string name, string ancestorName, int ancestorNth);
    IScriptStatement CreateDoesDocumentContainDescendantElementOfIdStatement(string domElementJson, string id, string errorMessage);
    IScriptStatement CreateCheckOrUncheckStatement(string domElementJson, bool check);
    IScriptStatement CreateInputStatement(string domElementJson, string text);
    IScriptStatement CreateDoesDocumentContainAnchorOrSubmitStatement(string ancestorDomElementJson, int anchorInstanceNumber);
    IScriptStatement CreateClickAnchorStatement(string domElementJson);
    IScriptStatement CreateIsOptionSelectedOrNot(string domElementJson, string option, bool selected);
}