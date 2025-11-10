using System.Text.Json;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class CheckOrUncheckSingleStep(IApplicationModel model,
        IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler,
        IOustScriptStatementFactory oustScriptStatementFactory,
        IWampLogScanner wampLogScanner, ScriptStepType stepType)
            : IScriptStepLogic {
    public IApplicationModel Model { get; init; } = model;
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; } = guiAndAppHandler;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        return await ShouldBeEnabledAsync();
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep { ScriptStepType = stepType };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        DateTime startOfExecutionTimeStamp = DateTime.Now;

        IScriptStatement scriptStatement = oustScriptStatementFactory.CreateDoesDocumentHaveNthOccurrenceOfIdOrClassStatement(Model.WithScriptStepIdOrClass, Model.WithScriptStepIdOrClassInstanceNumber);
        ScriptCallResponse scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        string ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = new ScriptStatement {
            Statement = "OustOccurrenceFinder.DoesDocumentContainSingleDescendantCheckBox(" + ancestorDomElementJson + ")",
            NoSuccessErrorMessage = string.Format(Properties.Resources.CheckBoxNotFoundOrNotUnique, Model.WithScriptStepIdOrClassInstanceNumber, Model.WithScriptStepIdOrClass)
        };
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        string domElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = oustScriptStatementFactory.CreateCheckOrUncheckStatement(domElementJson, stepType == ScriptStepType.CheckSingle);
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return; }

        wampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out string errorMessage);

        Model.Status.Text = errorMessage;
        Model.Status.Type = string.IsNullOrWhiteSpace(errorMessage) ? StatusType.None : StatusType.Error;
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.FormOrControlOrIdOrClassTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await Task.FromResult(new List<Selectable>());
    }
}