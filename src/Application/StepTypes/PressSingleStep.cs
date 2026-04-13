using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class PressSingleStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler,
        IWampLogScanner wampLogScanner, IOustScriptStatementFactory oustScriptStatementFactory)
            : IScriptStepLogic {
    public IApplicationModel Model { get; init; } = model;
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; } = guiAndAppHandler;
    public ISimpleLogger SimpleLogger { get; init; } = simpleLogger;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        return await ShouldBeEnabledAsync();
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep { ScriptStepType = ScriptStepType.PressSingle };
    }

    public async Task ExecuteAsync() {
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(PressSingleStep) + nameof(IScriptStepLogic.ExecuteAsync)))) {
            DateTime startOfExecutionTimeStamp = wampLogScanner.WaitUntilLogFolderIsErrorFreeReturnStartOfExecutionTimeStamp();
            IScriptStatement scriptStatement = oustScriptStatementFactory.CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber);
            ScriptCallResponse scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
                return;
            }

            string domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
            scriptStatement = oustScriptStatementFactory.CreateDoesDocumentContainAnchorOrSubmitStatement(domElementJson, 1);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
                return;
            }

            domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
            scriptStatement = oustScriptStatementFactory.CreateClickAnchorStatement(domElementJson);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
            if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
                return;
            }

            await GuiAndAppHandler.WaitUntilNotNavigatingAnymoreAsync();

            wampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out string errorMessage);

            await GuiAndAppHandler.WaitUntilNotNavigatingAnymoreAsync();

            Model.Status.Text = errorMessage;
            Model.Status.Type = string.IsNullOrWhiteSpace(errorMessage) ? StatusType.None : StatusType.Error;
        }
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await Task.FromResult(new List<Selectable>());
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.FormOrControlOrIdOrClassTitle;
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }
}