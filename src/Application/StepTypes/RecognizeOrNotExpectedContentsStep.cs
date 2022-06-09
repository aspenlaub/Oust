using System.Text.Json;
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
    private readonly IOucoHelper _OucoHelper;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;

    private readonly ScriptStepType _ScriptStepType;

    public string FreeCodeLabelText => _ScriptStepType == ScriptStepType.Recognize ? Properties.Resources.ExpectedContentsTitle : Properties.Resources.NotExpectedContentsTitle;

    public RecognizeOrNotExpectedContentsStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IOucoHelper oucoHelper, IOustScriptStatementFactory oustScriptStatementFactory, ScriptStepType stepType) {
        Model = model;
        GuiAndAppHandler = guiAndAppHandler;
        SimpleLogger = simpleLogger;
        _OucoHelper = oucoHelper;
        _OustScriptStatementFactory = oustScriptStatementFactory;
        _ScriptStepType = stepType;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!await ShouldBeEnabledAsync()) { return false; }

        return !string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid) || !string.IsNullOrWhiteSpace(Model.WithScriptStepIdOrClass);
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep {
            ScriptStepType = _ScriptStepType,
            ControlGuid = Model.ScriptStepOutOfControl?.Guid ?? "",
            ControlName = Model.ScriptStepOutOfControl?.Name ?? "",
            ExpectedContents = Model.ExpectedContents.Text
        };

    }

    public async Task<bool> ShouldBeEnabledAsync() {
        if (_ScriptStepType != ScriptStepType.Recognize && _ScriptStepType != ScriptStepType.NotExpectedContents) { return false; }
        return await Task.FromResult(true);
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.ControlTitle;
    }

    public async Task ExecuteAsync() {
        var scriptStatement = string.IsNullOrWhiteSpace(Model.WithScriptStepOucoOrOutrapForm?.Guid)
            ? _OustScriptStatementFactory.CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber)
            : _OustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.WithScriptStepOucoOrOutrapForm.Guid, Model.WithScriptStepOucoOrOutrapFormInstanceNumber, Model.WithScriptStepOucoOrOutrapForm.Name);

        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        var ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        var ancestorDomElementInnerHtml = scriptCallResponse.InnerHtml;
        string html;
        if (string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid)) {
            html = ancestorDomElementInnerHtml;
        } else {
            scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, Model.ScriptStepOutOfControl.Guid, Model.ScriptStepOutOfControl.Name, Model.WithScriptStepOucoOrOutrapForm?.Name, Model.WithScriptStepOucoOrOutrapFormInstanceNumber);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            html = scriptCallResponse.InnerHtml;
        }

        Model.Status.Text = "";
        Model.Status.Type = StatusType.None;

        if (Model.ExpectedContents.Text == "") { return; }

        if (_ScriptStepType == ScriptStepType.Recognize) {
            if (html.Contains(Model.ExpectedContents.Text)) { return; }
            if (html.Contains(Model.ExpectedContents.Text.Replace("'", "\""))) { return; }

            if (html.Length < 20) {
                Model.Status.Text = string.IsNullOrWhiteSpace(Model.WithScriptStepOucoOrOutrapForm?.Guid) ?
                    string.Format(Properties.Resources.ExpectedContentsNotFoundInstead, Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber, Model.WithScriptStepIdOrClass, Model.ExpectedContents.Text, html)
                    :
                    string.Format(Properties.Resources.ExpectedContentsNotFoundInstead, Model.ScriptStepOutOfControl?.Name, Model.WithScriptStepOucoOrOutrapFormInstanceNumber, Model.WithScriptStepOucoOrOutrapForm.Name, Model.ExpectedContents.Text, html);
            } else {
                Model.Status.Text = string.IsNullOrWhiteSpace(Model.WithScriptStepOucoOrOutrapForm?.Guid) ?
                    string.Format(Properties.Resources.ExpectedContentsNotFound, Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber, Model.WithScriptStepIdOrClass, Model.ExpectedContents.Text)
                    :
                    string.Format(Properties.Resources.ExpectedContentsNotFound, Model.ScriptStepOutOfControl?.Name, Model.WithScriptStepOucoOrOutrapFormInstanceNumber, Model.WithScriptStepOucoOrOutrapForm.Name, Model.ExpectedContents.Text);
            }
        } else if (html.Contains(Model.ExpectedContents.Text)) {
            Model.Status.Text = string.IsNullOrWhiteSpace(Model.WithScriptStepOucoOrOutrapForm?.Guid) ?
                string.Format(Properties.Resources.UnexpectedContentsFound, Model.WithScriptStepIdOrClassInstanceNumber, Model.WithScriptStepIdOrClass, Model.ExpectedContents.Text)
                :
                string.Format(Properties.Resources.UnexpectedContentsFound, Model.WithScriptStepOucoOrOutrapFormInstanceNumber, Model.WithScriptStepOucoOrOutrapForm.Name, Model.ExpectedContents.Text);

        } else {
            return;
        }

        Model.Status.Type = StatusType.Error;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await _OucoHelper.OutOfControlChoicesAsync(_ScriptStepType, Model.WithScriptStepOucoOrOutrapForm?.Guid, Model.WithScriptStepOucoOrOutrapFormInstanceNumber);
    }
}