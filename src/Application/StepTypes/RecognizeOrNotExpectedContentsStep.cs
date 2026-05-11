using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class RecognizeOrNotExpectedContentsStep(IApplicationModel model, ISimpleLogger simpleLogger,
        IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IOutrapHelper outrapHelper,
        IOustScriptStatementFactory oustScriptStatementFactory, ScriptStepType stepType)
            : IScriptStepLogic {
    public IApplicationModel Model { get; init; } = model;
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; } = guiAndAppHandler;
    public ISimpleLogger SimpleLogger { get; init; } = simpleLogger;

    public string FreeCodeLabelText => stepType == ScriptStepType.RecognizeSelection
        ? Properties.Resources.FreeTextTitle
        : stepType == ScriptStepType.Recognize
                ? Properties.Resources.ExpectedContentsTitle
                : Properties.Resources.NotExpectedContentsTitle;

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!await ShouldBeEnabledAsync()) { return false; }

        if ((stepType == ScriptStepType.EndScriptIfRecognized || stepType == ScriptStepType.EndScriptIfNotRecognized)
                && string.IsNullOrEmpty(Model.ExpectedContents.Text)) {
            return false;
        }

        return !string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid) || !string.IsNullOrWhiteSpace(Model.WithScriptStepIdOrClass);
    }

    public IScriptStep CreateScriptStepToAdd() {
        bool haveSelectedIndex = Model.FormOrControlOrIdOrClass.SelectedIndex >= 0;
        return new ScriptStep {
            ScriptStepType = stepType,
            ControlGuid = haveSelectedIndex ? Model.ScriptStepOutOfControl?.Guid ?? "" : "",
            ControlName = haveSelectedIndex ? Model.ScriptStepOutOfControl?.Name ?? "" : "",
            ExpectedContents = stepType == ScriptStepType.RecognizeSelection ? Model.SelectedValue.SelectedItem.Name : Model.ExpectedContents.Text
        };

    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(stepType == ScriptStepType.Recognize ||
             stepType == ScriptStepType.NotExpectedContents ||
             stepType == ScriptStepType.RecognizeSelection ||
             stepType == ScriptStepType.NotExpectedSelection ||
             stepType == ScriptStepType.EndScriptIfRecognized ||
             stepType == ScriptStepType.EndScriptIfNotRecognized
        );
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.ControlTitle;
    }

    public async Task ExecuteAsync() {
        IScriptStatement scriptStatement = string.IsNullOrWhiteSpace(Model.WithScriptStepOutrapForm?.Guid)
            ? oustScriptStatementFactory.CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber)
            : oustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.WithScriptStepOutrapForm.Guid, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name);

        ScriptCallResponse scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
            return;
        }

        string ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        string ancestorDomElementInnerHtml = scriptCallResponse.InnerHtml;
        string html;
        string domElementJson = ancestorDomElementJson;
        if (string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid)) {
            html = ancestorDomElementInnerHtml;
        } else {
            scriptStatement = oustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, Model.ScriptStepOutOfControl.Guid, Model.ScriptStepOutOfControl.Name, Model.WithScriptStepOutrapForm?.Name, Model.WithScriptStepOutrapFormInstanceNumber);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
            html = scriptCallResponse.InnerHtml;
        }

        Model.Status.Text = "";
        Model.Status.Type = StatusType.None;

        if (stepType != ScriptStepType.RecognizeSelection && Model.ExpectedContents.Text == "") { return; }
        if (stepType == ScriptStepType.RecognizeSelection && !Model.SelectedValue.SelectionMade) { return; }

        switch (stepType) {
            case ScriptStepType.RecognizeSelection or ScriptStepType.NotExpectedSelection: {
                scriptStatement = oustScriptStatementFactory.CreateIsOptionSelectedOrNot(domElementJson,
                    stepType == ScriptStepType.RecognizeSelection ? Model.SelectedValue.SelectedItem.Guid : Model.ExpectedContents.Text,
                    stepType == ScriptStepType.RecognizeSelection);
                scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
                if (scriptCallResponse.Success.Inconclusive || scriptCallResponse.Success.YesNo) { return; }

                Model.Status.Text = stepType == ScriptStepType.RecognizeSelection
                    ? Properties.Resources.AnotherValueIsSelected
                    : Properties.Resources.ValueIsSelectedThisIsUnexpected;
                break;
            }
            case ScriptStepType.Recognize when html.Contains(Model.ExpectedContents.Text):
            case ScriptStepType.Recognize when html.Contains(Model.ExpectedContents.Text.Replace("'", "\"")):
            case ScriptStepType.Recognize when html.Contains(Model.ExpectedContents.Text.Replace("\\n", "\n")):
            case ScriptStepType.EndScriptIfRecognized when html.Contains(Model.ExpectedContents.Text):
            case ScriptStepType.EndScriptIfRecognized when html.Contains(Model.ExpectedContents.Text.Replace("'", "\"")):
                if (stepType == ScriptStepType.EndScriptIfRecognized) {
                    Model.Status.Text = Properties.Resources.ContentsFoundEndScript;
                }
                return;
            case ScriptStepType.EndScriptIfNotRecognized when !html.Contains(Model.ExpectedContents.Text)
                    && !html.Contains(Model.ExpectedContents.Text.Replace("'", "\"")):
                Model.Status.Text = Properties.Resources.ContentsNotFoundEndScript;
                return;
            case ScriptStepType.Recognize when html.Length < 20:
                Model.Status.Text = string.IsNullOrWhiteSpace(Model.WithScriptStepOutrapForm?.Guid)
                    ? string.Format(Properties.Resources.ExpectedContentsNotFoundInstead, Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber, Model.WithScriptStepIdOrClass, Model.ExpectedContents.Text, html)
                    : string.Format(Properties.Resources.ExpectedContentsNotFoundInstead, Model.ScriptStepOutOfControl?.Name, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name, Model.ExpectedContents.Text, html);
                break;
            case ScriptStepType.Recognize:
            case ScriptStepType.EndScriptIfRecognized:
                if (stepType == ScriptStepType.EndScriptIfRecognized) { return; }

                Model.Status.Text = string.IsNullOrWhiteSpace(Model.WithScriptStepOutrapForm?.Guid)
                    ? string.Format(Properties.Resources.ExpectedContentsNotFound, Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber, Model.WithScriptStepIdOrClass, Model.ExpectedContents.Text)
                    : string.Format(Properties.Resources.ExpectedContentsNotFound, Model.ScriptStepOutOfControl?.Name, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name, Model.ExpectedContents.Text);
                break;
            case ScriptStepType.EndScriptIfNotRecognized:
                return;
            default: {
                if (html.Contains(Model.ExpectedContents.Text)) {
                    Model.Status.Text = string.IsNullOrWhiteSpace(Model.WithScriptStepOutrapForm?.Guid)
                        ? string.Format(Properties.Resources.UnexpectedContentsFound, Model.WithScriptStepIdOrClassInstanceNumber, Model.WithScriptStepIdOrClass, Model.ExpectedContents.Text)
                        : string.Format(Properties.Resources.UnexpectedContentsFound, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name, Model.ExpectedContents.Text);

                } else {
                    return;
                }

                break;
            }
        }

        Model.Status.Type = StatusType.Error;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await outrapHelper.OutOfControlChoicesAsync(stepType, Model.WithScriptStepOutrapForm?.Guid, Model.WithScriptStepOutrapFormInstanceNumber);
    }
}