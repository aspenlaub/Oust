using System.Text.Json;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;

public class OustScriptStatementFactory : IOustScriptStatementFactory {
    public IScriptStatement CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(string idOrClass, int n) {
        return new ScriptStatement {
            Statement = "OustOccurrenceFinder.DoesDocumentHaveNthOccurrenceOfIdOrClass(\"" + idOrClass + "\", " + n + ")",
            NoSuccessErrorMessage = !string.IsNullOrWhiteSpace(idOrClass)
                ? string.Format(Properties.Resources.InstanceXOfYNotFound, n, idOrClass)
                : string.Format(Properties.Resources.NoIdOrClassContext),
            NoFailureErrorMessage = !string.IsNullOrWhiteSpace(idOrClass)
                ? string.Format(Properties.Resources.UnexpectedInstanceXOfYFound, idOrClass, n)
                : string.Format(Properties.Resources.NoIdOrClassContext)
        };
    }

    public IScriptStatement CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(string idOrClass, int n, string name) {
        return new ScriptStatement {
            Statement = "OustOccurrenceFinder.DoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClass(\"" + idOrClass + "\", " + n + ")",
            InconclusiveErrorMessage = name != ""
                ? string.Format(Properties.Resources.CouldNotDetermineIfInstanceXOfYIsThereOrNot, n, name)
                : string.Format(Properties.Resources.CouldNotDetermineFormContext),
            NoSuccessErrorMessage = name != "" ? string.Format(Properties.Resources.InstanceXOfYNotFound, n, name) : string.Format(Properties.Resources.NoFormContext)
        };
    }

    public IScriptStatement CreateDoesDocumentContainDescendantElementOfClassStatement(string ancestorDomElementJson, string className, string name, string ancestorName, int ancestorNth) {
        return new ScriptStatement {
            Statement = "OustOccurrenceFinder.DoesDocumentContainDescendantElementOfClass(" + ancestorDomElementJson + ", \"" + className + "\")",
            NoSuccessErrorMessage = string.Format(Properties.Resources.ControlNotFound, name, ancestorNth, ancestorName)
        };
    }

    public IScriptStatement CreateDoesDocumentContainDescendantElementOfIdStatement(string domElementJson, string id, string errorMessage) {
        var scriptStatement = new ScriptStatement {
            Statement = "OustOccurrenceFinder.DoesDocumentContainDescendantElementOfId(" + domElementJson + ", \"" + id + "\")"
        };

        if (!string.IsNullOrEmpty(errorMessage)) {
            scriptStatement.NoSuccessErrorMessage = errorMessage;
        }

        return scriptStatement;
    }

    public IScriptStatement CreateCheckOrUncheckStatement(string domElementJson, bool check) {
        return new ScriptStatement {
            Statement = "OustActions.CheckOrUncheck(" + domElementJson + ", " + (check ? "true" : "false") + ")"
        };
    }

    public IScriptStatement CreateInputStatement(string domElementJson, string text) {
        return new ScriptStatement {
            Statement = "OustActions.Input(" + domElementJson + ", " + JsonSerializer.Serialize(text) + ")"
        };
    }

    public IScriptStatement CreateDoesDocumentContainAnchorOrSubmitStatement(string ancestorDomElementJson, int anchorInstanceNumber) {
        return new ScriptStatement {
            Statement = "OustOccurrenceFinder.DoesDocumentContainAnchorOrSubmit(" + ancestorDomElementJson + ", " + anchorInstanceNumber + ")"
        };
    }

    public IScriptStatement CreateClickAnchorStatement(string domElementJson) {
        return new ScriptStatement {
            Statement = "OustActions.ClickAnchor(" + domElementJson + ")",
            NoSuccessErrorMessage = Properties.Resources.ElementNotFoundOrNotAnAnchor
        };
    }

    public IScriptStatement CreateIsOptionSelectedOrNot(string domElementJson, string option, bool selected) {
        return new ScriptStatement {
            Statement = "OustUtilities.IsOptionSelectedOrNot(" + domElementJson + ", " + JsonSerializer.Serialize(option) + ", " + (selected ? "true" : "false") + ")",
            InconclusiveErrorMessage = Properties.Resources.CouldNotDetermineSelectedOption,
            NoSuccessErrorMessage = string.Format(selected ? Properties.Resources.OptionIsNotSelected : Properties.Resources.OptionIsSelected, option)
        };
    }
}