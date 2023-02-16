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

public class InputStep : IScriptStepLogic {
    public IApplicationModel Model { get; init; }
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; }
    public ISimpleLogger SimpleLogger { get; init; }
    private readonly IOutrapHelper _OutrapHelper;
    private readonly IFileDialogTrickster _FileDialogTrickster;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;
    private readonly IOustSettingsHelper _OustSettingsHelper;
    private readonly ScriptStepType _ScriptStepType;

    public string FreeCodeLabelText => _ScriptStepType == ScriptStepType.Input ? Properties.Resources.InputTitle : Properties.Resources.FreeTextTitle;

    public InputStep(ScriptStepType scriptStepType,  IApplicationModel model, ISimpleLogger simpleLogger,
            IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IOutrapHelper outrapHelper, IFileDialogTrickster fileDialogTrickster,
            IOustScriptStatementFactory oustScriptStatementFactory, IOustSettingsHelper oustSettingsHelper) {
        Model = model;
        GuiAndAppHandler = guiAndAppHandler;
        SimpleLogger = simpleLogger;
        _OutrapHelper = outrapHelper;
        _FileDialogTrickster = fileDialogTrickster;
        _OustScriptStatementFactory = oustScriptStatementFactory;
        _OustSettingsHelper = oustSettingsHelper;
        _ScriptStepType = scriptStepType;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        if (!await ShouldBeEnabledAsync()) { return false; }

        switch (_ScriptStepType) {
            case ScriptStepType.ClearInput:
                return !string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid);
            default:
                return !string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid) && !string.IsNullOrWhiteSpace(Model.ScriptStepInput.Text);
        }
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep {
            ScriptStepType = _ScriptStepType,
            ControlGuid = Model.ScriptStepOutOfControl.Guid,
            ControlName = Model.ScriptStepOutOfControl.Name,
            InputText = _ScriptStepType == ScriptStepType.Input ? Model.ScriptStepInput.Text : ""
        };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.WithScriptStepOutrapForm.Guid, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name);
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        var ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);

        scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, Model.ScriptStepOutOfControl.Guid, Model.ScriptStepOutOfControl.Name, Model.WithScriptStepOutrapForm.Name, Model.WithScriptStepOutrapFormInstanceNumber);
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
            return;
        }

        var isTextArea = scriptCallResponse.DomElement.IsTextArea.YesNo;
        var domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);

        scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfIdStatement(domElementJson, "selectuploadfile", "");
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        var isUpload = scriptCallResponse.Success.YesNo && !scriptCallResponse.Success.Inconclusive;

        var scriptStepInputText = _ScriptStepType == ScriptStepType.Input ? Model.ScriptStepInput.Text : "";
        var isWindows11 = (await _OustSettingsHelper.ShouldWindows11BeAssumedAsync()).YesNo;
        if (isTextArea) {
            scriptStepInputText = scriptStepInputText.Replace(@"\n", "\n");
        } else if (isUpload && isWindows11) {
            scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfIdStatement(domElementJson, "uploadfiledisplayed", Properties.Resources.ThisIsWindows11NeedUploadFileDisplayedInputButNotFound);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            scriptStepInputText = "bypass:" + scriptStepInputText;
            domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        }

        if (isUpload && !isWindows11) {
            scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainAnchorOrSubmitStatement(domElementJson, 1);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
            scriptStatement = _OustScriptStatementFactory.CreateClickAnchorStatement(domElementJson);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            var errorMessage = "";
            await Task.Run(async () => {
                var windowName = Model.EnvironmentType == EnvironmentType.UnitTest ? Properties.Resources.OustUnitTestWindowName : Properties.Resources.OustWindowName;
                errorMessage = await _FileDialogTrickster.EnterFileNameAndHaveOpenButtonPressedReturnErrorMessageAsync(scriptStepInputText, windowName);
            });

            Model.Status.Text = errorMessage;
            Model.Status.Type = string.IsNullOrWhiteSpace(errorMessage) ? StatusType.None : StatusType.Error;
            return;
        }

        scriptStatement = _OustScriptStatementFactory.CreateInputStatement(domElementJson, scriptStepInputText);
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        Model.Status.Text = "";
        Model.Status.Type = StatusType.None;
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.ControlTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await _OutrapHelper.OutOfControlChoicesAsync(_ScriptStepType, Model.WithScriptStepOutrapForm?.Guid, Model.WithScriptStepOutrapFormInstanceNumber);
    }
}