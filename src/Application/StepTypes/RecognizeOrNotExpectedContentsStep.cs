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

public class RecognizeOrNotExpectedContentsStep : IScriptStepLogic {
    public IApplicationModel Model { get; init; }
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; }
    public ISimpleLogger SimpleLogger { get; init; }
    private readonly IOutrapHelper _OutrapHelper;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;

    private readonly ScriptStepType _ScriptStepType;

    public string FreeCodeLabelText => _ScriptStepType == ScriptStepType.RecognizeSelection
        ? Properties.Resources.FreeTextTitle
        : _ScriptStepType == ScriptStepType.Recognize
                ? Properties.Resources.ExpectedContentsTitle
                : Properties.Resources.NotExpectedContentsTitle;

    public RecognizeOrNotExpectedContentsStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IOutrapHelper outrapHelper, IOustScriptStatementFactory oustScriptStatementFactory, ScriptStepType stepType) {
        Model = model;
        GuiAndAppHandler = guiAndAppHandler;
        SimpleLogger = simpleLogger;
        _OutrapHelper = outrapHelper;
        _OustScriptStatementFactory = oustScriptStatementFactory;
        _ScriptStepType = stepType;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!await ShouldBeEnabledAsync()) { return false; }

        if (_ScriptStepType == ScriptStepType.EndScriptIfRecognized && string.IsNullOrEmpty(Model.ExpectedContents.Text)) {
            return false;
        }

        return !string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid) || !string.IsNullOrWhiteSpace(Model.WithScriptStepIdOrClass);
    }

    public IScriptStep CreateScriptStepToAdd() {
        bool haveSelectedIndex = Model.FormOrControlOrIdOrClass.SelectedIndex >= 0;
        return new ScriptStep {
            ScriptStepType = _ScriptStepType,
            ControlGuid = haveSelectedIndex ? Model.ScriptStepOutOfControl?.Guid ?? "" : "",
            ControlName = haveSelectedIndex ? Model.ScriptStepOutOfControl?.Name ?? "" : "",
            ExpectedContents = _ScriptStepType == ScriptStepType.RecognizeSelection ? Model.SelectedValue.SelectedItem.Name : Model.ExpectedContents.Text
        };

    }

    public async Task<bool> ShouldBeEnabledAsync() {
        if (_ScriptStepType != ScriptStepType.Recognize && _ScriptStepType != ScriptStepType.NotExpectedContents
            && _ScriptStepType != ScriptStepType.RecognizeSelection && _ScriptStepType != ScriptStepType.NotExpectedSelection
            && _ScriptStepType != ScriptStepType.EndScriptIfRecognized) { return false; }
        return await Task.FromResult(true);
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.ControlTitle;
    }

    public async Task ExecuteAsync() {
        IScriptStatement scriptStatement = string.IsNullOrWhiteSpace(Model.WithScriptStepOutrapForm?.Guid)
            ? _OustScriptStatementFactory.CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber)
            : _OustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.WithScriptStepOutrapForm.Guid, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name);

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
            scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, Model.ScriptStepOutOfControl.Guid, Model.ScriptStepOutOfControl.Name, Model.WithScriptStepOutrapForm?.Name, Model.WithScriptStepOutrapFormInstanceNumber);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
            html = scriptCallResponse.InnerHtml;
        }

        Model.Status.Text = "";
        Model.Status.Type = StatusType.None;

        if (_ScriptStepType != ScriptStepType.RecognizeSelection && Model.ExpectedContents.Text == "") { return; }
        if (_ScriptStepType == ScriptStepType.RecognizeSelection && !Model.SelectedValue.SelectionMade) { return; }

        switch (_ScriptStepType) {
            case ScriptStepType.RecognizeSelection or ScriptStepType.NotExpectedSelection: {
                scriptStatement = _OustScriptStatementFactory.CreateIsOptionSelectedOrNot(domElementJson,
                    _ScriptStepType == ScriptStepType.RecognizeSelection ? Model.SelectedValue.SelectedItem.Guid : Model.ExpectedContents.Text,
                    _ScriptStepType == ScriptStepType.RecognizeSelection);
                scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
                if (scriptCallResponse.Success.Inconclusive || scriptCallResponse.Success.YesNo) { return; }

                Model.Status.Text = _ScriptStepType == ScriptStepType.RecognizeSelection
                    ? Properties.Resources.AnotherValueIsSelected
                    : Properties.Resources.ValueIsSelectedThisIsUnexpected;
                break;
            }
            case ScriptStepType.Recognize when html.Contains(Model.ExpectedContents.Text):
            case ScriptStepType.Recognize when html.Contains(Model.ExpectedContents.Text.Replace("'", "\"")):
            case ScriptStepType.Recognize when html.Contains(Model.ExpectedContents.Text.Replace("\\n", "\n")):
            case ScriptStepType.EndScriptIfRecognized when html.Contains(Model.ExpectedContents.Text):
            case ScriptStepType.EndScriptIfRecognized when html.Contains(Model.ExpectedContents.Text.Replace("'", "\"")):
                if (_ScriptStepType == ScriptStepType.EndScriptIfRecognized) {
                    Model.Status.Text = Properties.Resources.ContentsFoundEndScript;
                }
                return;
            case ScriptStepType.Recognize when html.Length < 20:
                Model.Status.Text = string.IsNullOrWhiteSpace(Model.WithScriptStepOutrapForm?.Guid)
                    ? string.Format(Properties.Resources.ExpectedContentsNotFoundInstead, Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber, Model.WithScriptStepIdOrClass, Model.ExpectedContents.Text, html)
                    : string.Format(Properties.Resources.ExpectedContentsNotFoundInstead, Model.ScriptStepOutOfControl?.Name, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name, Model.ExpectedContents.Text, html);
                break;
            case ScriptStepType.Recognize:
            case ScriptStepType.EndScriptIfRecognized:
                if (_ScriptStepType == ScriptStepType.EndScriptIfRecognized) { return; }

                Model.Status.Text = string.IsNullOrWhiteSpace(Model.WithScriptStepOutrapForm?.Guid)
                    ? string.Format(Properties.Resources.ExpectedContentsNotFound, Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber, Model.WithScriptStepIdOrClass, Model.ExpectedContents.Text)
                    : string.Format(Properties.Resources.ExpectedContentsNotFound, Model.ScriptStepOutOfControl?.Name, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name, Model.ExpectedContents.Text);
                break;
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
        return await _OutrapHelper.OutOfControlChoicesAsync(_ScriptStepType, Model.WithScriptStepOutrapForm?.Guid, Model.WithScriptStepOutrapFormInstanceNumber);
    }
}