using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class PressStep(IApplicationModel model, ISimpleLogger simpleLogger,
        IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler,
        IOutrapHelper outrapHelper, IWampLogScanner wampLogScanner,
        IOustScriptStatementFactory oustScriptStatementFactory)
            : IScriptStepLogic {
    public IApplicationModel Model { get; init; } = model;
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; } = guiAndAppHandler;
    public ISimpleLogger SimpleLogger { get; init; } = simpleLogger;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        return await ShouldBeEnabledAsync()
               && !string.IsNullOrWhiteSpace(Model.ScriptStepOutOfControl?.Guid);
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep {
            ScriptStepType = ScriptStepType.Press,
            ControlGuid = Model.ScriptStepOutOfControl.Guid,
            ControlName = Model.ScriptStepOutOfControl.Name
        };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(PressStep) + nameof(IScriptStepLogic.ExecuteAsync)))) {
            DateTime startOfExecutionTimeStamp = wampLogScanner.WaitUntilLogFolderIsErrorFreeReturnStartOfExecutionTimeStamp();

            IScriptStatement scriptStatement = oustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(Model.WithScriptStepOutrapForm.Guid, Model.WithScriptStepOutrapFormInstanceNumber, Model.WithScriptStepOutrapForm.Name);
            ScriptCallResponse scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            string ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);

            scriptStatement = oustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, Model.ScriptStepOutOfControl.Guid, Model.ScriptStepOutOfControl.Name, Model.WithScriptStepOutrapForm.Name, Model.WithScriptStepOutrapFormInstanceNumber);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
                return;
            }

            string outerDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
            scriptStatement = oustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfIdStatement(outerDomElementJson, "selectuploadfile", "");
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
            bool isUpload = scriptCallResponse.Success.YesNo && !scriptCallResponse.Success.Inconclusive;

            scriptStatement = oustScriptStatementFactory.CreateDoesDocumentContainAnchorOrSubmitStatement(outerDomElementJson, isUpload ? 2 : 1);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            string uploadFileDisplayed = "";
            if (isUpload) {
                IScriptStatement scriptStatement2 = oustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfIdStatement(outerDomElementJson, "uploadfiledisplayed", "");
                ScriptCallResponse scriptCallResponse2 = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement2, true, true);
                if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }
                uploadFileDisplayed = scriptCallResponse2.InnerHtml;
            }

            string innerDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
            scriptStatement = oustScriptStatementFactory.CreateClickAnchorStatement(innerDomElementJson);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

            await GuiAndAppHandler.WaitUntilNotNavigatingAnymoreAsync();

            if (isUpload) {
                await Wait.UntilAsync(async () => {
                    IScriptStatement scriptStatement2 = oustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfIdStatement(outerDomElementJson, "uploadfiledisplayed", "");
                    ScriptCallResponse scriptCallResponse2 = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement2, true, true);
                    return !scriptCallResponse.Success.Inconclusive
                           && scriptCallResponse.Success.YesNo
                           && scriptCallResponse2.InnerHtml != uploadFileDisplayed;
                }, TimeSpan.FromSeconds(30));
            }

            wampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out string errorMessage);

            await GuiAndAppHandler.WaitUntilNotNavigatingAnymoreAsync();

            Model.Status.Text = errorMessage;
            Model.Status.Type = string.IsNullOrWhiteSpace(errorMessage) ? StatusType.None : StatusType.Error;
        }
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.ControlTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await outrapHelper.OutOfControlChoicesAsync(ScriptStepType.Press, Model.WithScriptStepOutrapForm?.Guid, Model.WithScriptStepOutrapFormInstanceNumber);
    }
}