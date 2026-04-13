using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Entities.Web;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Web;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class GoToUrlStep(IApplicationModel model, ISimpleLogger simpleLogger, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler,
        IWampLogScanner wampLogScanner, ISecuredHttpGate securedHttpGate, ISecretRepository secretRepository,
        IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor, bool justInvoke)
            : IScriptStepLogic {
    public string FreeCodeLabelText => Properties.Resources.UrlTitle;

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        return await Task.FromResult(ShouldBeEnabled());
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep { ScriptStepType = justInvoke ? ScriptStepType.InvokeUrl : ScriptStepType.GoToUrl, Url = model.ScriptStepUrl.Text };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(ShouldBeEnabled());
    }

    private bool ShouldBeEnabled() {
        if (!model.SelectedScript.SelectionMade) { return false; }

        return model.SelectedScript.SelectedItem.Name != Script.NewScriptName && model.ScriptStepUrl.Text.StartsWith("http://localhost/");
    }

    public async Task ExecuteAsync() {
        using (simpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(GoToUrlStep) + nameof(IScriptStepLogic.ExecuteAsync)))) {
            IList<string> methodNamesFromStack = methodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();

            if (!justInvoke) {
                model.WithScriptStepOutrapForm = null;
                model.WithScriptStepIdOrClass = null;
                model.WithScriptStepOutOfControl = null;
                model.WithScriptStepOutrapFormInstanceNumber = 0;
                model.WithScriptStepIdOrClassInstanceNumber = 0;
            }

            DateTime startOfExecutionTimeStamp = wampLogScanner.WaitUntilLogFolderIsErrorFreeReturnStartOfExecutionTimeStamp();
            string errorMessage;

            if (justInvoke) {
                simpleLogger.LogInformationWithCallStack($"Invoking {model.ScriptStepUrl.Text}", methodNamesFromStack);
            } else {
                string url = model.ScriptStepUrl.Text;
                NavigationResult result = await guiAndAppHandler.NavigateToUrlAsync(url);
                if (!result.Succeeded) {
                    simpleLogger.LogInformationWithCallStack($"Could not navigate to {url}", methodNamesFromStack);
                    model.Status.Text = string.Format(Properties.Resources.CouldNotNavigateToUrl, url);
                    model.Status.Type = StatusType.Error;
                    return;
                }

                simpleLogger.LogInformationWithCallStack($"App navigated to {model.ScriptStepUrl.Text}", methodNamesFromStack);

                OucidResponse oucidAggregatedResponse = result.OucidResponse;
                if (oucidAggregatedResponse.WaitForLocalhostLogs) {
                    simpleLogger.LogInformationWithCallStack("Waiting for stable log folder", methodNamesFromStack);
                    guiAndAppHandler.IndicateBusy(true);

                    wampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out errorMessage);
                    if (errorMessage != "") {
                        simpleLogger.LogInformationWithCallStack("Error in wamp log", methodNamesFromStack);
                        model.Status.Text = errorMessage;
                        model.Status.Type = StatusType.Error;
                        return;
                    }
                }

                startOfExecutionTimeStamp = DateTime.Now;

                if (oucidAggregatedResponse.WaitUntilNotNavigating) {
                    simpleLogger.LogInformationWithCallStack("Waiting for navigation end", methodNamesFromStack);
                    await guiAndAppHandler.WaitUntilNotNavigatingAnymoreAsync();
                }

                if (oucidAggregatedResponse.BasicValidation) {
                    simpleLogger.LogInformationWithCallStack("Validating deprecated bootstrap classes", methodNamesFromStack);

                    var scriptStatement = new ScriptStatement {
                        Statement = "OustUtilities.DoesDocumentContainDeprecatedBootstrapClasses()",
                        NoSuccessErrorMessage = Properties.Resources.CouldNotVerifyIfDocumentContainsDeprecatedBootstrapClasses
                    };
                    ScriptCallResponse scriptCallResponse = await guiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
                    if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        scriptCallResponse = await guiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
                        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
                            return;
                        }
                    }
                    if (scriptCallResponse.Dictionary.Keys.Count != 0) {
                        simpleLogger.LogInformationWithCallStack("Deprecated bootstrap class/-es found", methodNamesFromStack);
                        model.Status.Text = string.Format(Properties.Resources.DocumentContainsDeprecatedBootstrapClasses, "." + string.Join(", .", scriptCallResponse.Dictionary.Values));
                        model.Status.Type = StatusType.Error;
                        return;
                    }
                }

                guiAndAppHandler.IndicateBusy(true);

                if (oucidAggregatedResponse.WaitAgainForLocalhostLogs) {
                    simpleLogger.LogInformationWithCallStack("App waiting for stable log folder", methodNamesFromStack);
                    wampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out errorMessage);
                    if (errorMessage != "") {
                        simpleLogger.LogInformationWithCallStack("Error in wamp log", methodNamesFromStack);
                        model.Status.Text = errorMessage;
                        model.Status.Type = StatusType.Error;
                        return;
                    }
                }

                if (oucidAggregatedResponse.WaitAgainUntilNotNavigating) {
                    simpleLogger.LogInformationWithCallStack("Waiting for navigation end", methodNamesFromStack);
                    await guiAndAppHandler.WaitUntilNotNavigatingAnymoreAsync();
                }

                guiAndAppHandler.IndicateBusy(true);

                if (oucidAggregatedResponse.MarkupValidation) {
                    if (!await DoWebClientValidationAsync()) {
                        model.Status.Text = string.Format(Properties.Resources.OucidResponseShouldNotSayMarkupValidation, url);
                        model.Status.Type = StatusType.Error;
                        return;
                    }
                    simpleLogger.LogInformationWithCallStack("App validating markup", methodNamesFromStack);
                    try {
                        HtmlValidationResult validationResult = await securedHttpGate.IsHtmlMarkupValidAsync(model.WebViewContentSource.Text);
                        if (!validationResult.Success) {
                            simpleLogger.LogInformationWithCallStack(Properties.Resources.MarkupValidationFailed, methodNamesFromStack);
                            model.Status.Text = validationResult.ErrorMessage;
                            model.Status.Type = StatusType.Error;
                            return;
                        }
                    } catch {
                        simpleLogger.LogInformationWithCallStack(Properties.Resources.MarkupValidationFailed, methodNamesFromStack);
                        model.Status.Text = Properties.Resources.MarkupValidationFailed;
                        model.Status.Type = StatusType.Error;
                        return;
                    }
                }
            }

            if (justInvoke) {
                using var client = new HttpClient();
                try {
                    string markup;
                    using (HttpResponseMessage response = await client.GetAsync(model.ScriptStepUrl.Text)) {
                        using (HttpContent content = response.Content) {
                            markup = await content.ReadAsStringAsync();
                        }
                    }
                    HtmlValidationResult validationResult = await securedHttpGate.IsHtmlMarkupValidAsync(markup);
                    if (!validationResult.Success) {
                        simpleLogger.LogInformationWithCallStack(Properties.Resources.MarkupValidationFailed, methodNamesFromStack);
                        model.Status.Text = validationResult.ErrorMessage;
                        model.Status.Type = StatusType.Error;
                        return;
                    }
                } catch {
                    simpleLogger.LogInformationWithCallStack(Properties.Resources.MarkupValidationFailed, methodNamesFromStack);
                    model.Status.Text = Properties.Resources.MarkupValidationFailed;
                    model.Status.Type = StatusType.Error;
                    return;
                }
            }

            wampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out errorMessage);
            if (errorMessage != "") {
                simpleLogger.LogInformationWithCallStack("Error in wamp log", methodNamesFromStack);
                model.Status.Text = errorMessage;
                model.Status.Type = StatusType.Error;
                return;
            }

            simpleLogger.LogInformationWithCallStack($"{model.ScriptStepUrl.Text} loaded and validated", methodNamesFromStack);

            model.Status.Text = "";
            model.Status.Type = StatusType.None;
        }
    }

    private async Task<bool> DoWebClientValidationAsync() {
        var secretDenyWebClientValidationCriteria = new DenyWebClientValidationCriteriaSecret();
        var errorsAndInfos = new ErrorsAndInfos();
        DenyWebClientValidationCriteria criteria = await secretRepository.GetAsync(secretDenyWebClientValidationCriteria, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            return true;
        }

        return !criteria.Any(c => model.ScriptStepUrl.Text.Contains(c.UrlPart));
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.FormOrControlOrIdOrClassTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await Task.FromResult(new List<Selectable>());
    }
}