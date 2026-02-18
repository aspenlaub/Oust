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
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Web;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class GoToUrlStep : IScriptStepLogic {
    private readonly IApplicationModel _Model;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;
    private readonly ISimpleLogger _SimpleLogger;
    private readonly IWampLogScanner _WampLogScanner;
    private readonly ISecuredHttpGate _SecuredHttpGate;
    private readonly ISecretRepository _SecretRepository;
    private readonly bool _JustInvoke;
    private readonly IMethodNamesFromStackFramesExtractor _MethodNamesFromStackFramesExtractor;

    public string FreeCodeLabelText => Properties.Resources.UrlTitle;

    public GoToUrlStep(IApplicationModel model, ISimpleLogger simpleLogger,
                IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IWampLogScanner wampLogScanner, ISecuredHttpGate securedHttpGate,
                ISecretRepository secretRepository,
                IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor, bool justInvoke) {
        _Model = model;
        _SimpleLogger = simpleLogger;
        _GuiAndAppHandler = guiAndAppHandler;
        _WampLogScanner = wampLogScanner;
        _SecuredHttpGate = securedHttpGate;
        _SecretRepository = secretRepository;
        _JustInvoke = justInvoke;
        _MethodNamesFromStackFramesExtractor = methodNamesFromStackFramesExtractor;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        return await Task.FromResult(ShouldBeEnabled());
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep { ScriptStepType = _JustInvoke ? ScriptStepType.InvokeUrl : ScriptStepType.GoToUrl, Url = _Model.ScriptStepUrl.Text };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(ShouldBeEnabled());
    }

    private bool ShouldBeEnabled() {
        if (!_Model.SelectedScript.SelectionMade) { return false; }

        return _Model.SelectedScript.SelectedItem.Name != Script.NewScriptName && _Model.ScriptStepUrl.Text.StartsWith("http://localhost/");
    }

    public async Task ExecuteAsync() {
        using (_SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(GoToUrlStep) + nameof(IScriptStepLogic.ExecuteAsync)))) {
            var methodNamesFromStack = _MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();

            if (!_JustInvoke) {
                _Model.WithScriptStepOutrapForm = null;
                _Model.WithScriptStepIdOrClass = null;
                _Model.WithScriptStepOutOfControl = null;
                _Model.WithScriptStepOutrapFormInstanceNumber = 0;
                _Model.WithScriptStepIdOrClassInstanceNumber = 0;
            }

            var startOfExecutionTimeStamp = DateTime.Now;
            string errorMessage;

            if (_JustInvoke) {
                _SimpleLogger.LogInformationWithCallStack($"Invoking {_Model.ScriptStepUrl.Text}", methodNamesFromStack);
            } else {
                var url = _Model.ScriptStepUrl.Text;
                var result = await _GuiAndAppHandler.NavigateToUrlAsync(url);
                if (!result.Succeeded) {
                    _SimpleLogger.LogInformationWithCallStack($"Could not navigate to {url}", methodNamesFromStack);
                    _Model.Status.Text = string.Format(Properties.Resources.CouldNotNavigateToUrl, url);
                    _Model.Status.Type = StatusType.Error;
                    return;
                }

                _SimpleLogger.LogInformationWithCallStack($"App navigated to {_Model.ScriptStepUrl.Text}", methodNamesFromStack);

                var oucidAggregatedResponse = result.OucidResponse;
                if (oucidAggregatedResponse.WaitForLocalhostLogs) {
                    _SimpleLogger.LogInformationWithCallStack("Waiting for stable log folder", methodNamesFromStack);
                    _GuiAndAppHandler.IndicateBusy(true);

                    _WampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out errorMessage);
                    if (errorMessage != "") {
                        _SimpleLogger.LogInformationWithCallStack("Error in wamp log", methodNamesFromStack);
                        _Model.Status.Text = errorMessage;
                        _Model.Status.Type = StatusType.Error;
                        return;
                    }
                }

                startOfExecutionTimeStamp = DateTime.Now;

                if (oucidAggregatedResponse.WaitUntilNotNavigating) {
                    _SimpleLogger.LogInformationWithCallStack("Waiting for navigation end", methodNamesFromStack);
                    await _GuiAndAppHandler.WaitUntilNotNavigatingAnymoreAsync();
                }

                if (oucidAggregatedResponse.BasicValidation) {
                    _SimpleLogger.LogInformationWithCallStack("Validating deprecated bootstrap classes", methodNamesFromStack);

                    var scriptStatement = new ScriptStatement {
                        Statement = "OustUtilities.DoesDocumentContainDeprecatedBootstrapClasses()",
                        NoSuccessErrorMessage = Properties.Resources.CouldNotVerifyIfDocumentContainsDeprecatedBootstrapClasses
                    };
                    var scriptCallResponse = await _GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
                    if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        scriptCallResponse = await _GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
                        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
                            return;
                        }
                    }
                    if (scriptCallResponse.Dictionary.Keys.Count != 0) {
                        _SimpleLogger.LogInformationWithCallStack("Deprecated bootstrap class/-es found", methodNamesFromStack);
                        _Model.Status.Text = string.Format(Properties.Resources.DocumentContainsDeprecatedBootstrapClasses, "." + string.Join(", .", scriptCallResponse.Dictionary.Values));
                        _Model.Status.Type = StatusType.Error;
                        return;
                    }
                }

                _GuiAndAppHandler.IndicateBusy(true);

                if (oucidAggregatedResponse.WaitAgainForLocalhostLogs) {
                    _SimpleLogger.LogInformationWithCallStack("App waiting for stable log folder", methodNamesFromStack);
                    _WampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out errorMessage);
                    if (errorMessage != "") {
                        _SimpleLogger.LogInformationWithCallStack("Error in wamp log", methodNamesFromStack);
                        _Model.Status.Text = errorMessage;
                        _Model.Status.Type = StatusType.Error;
                        return;
                    }
                }

                if (oucidAggregatedResponse.WaitAgainUntilNotNavigating) {
                    _SimpleLogger.LogInformationWithCallStack("Waiting for navigation end", methodNamesFromStack);
                    await _GuiAndAppHandler.WaitUntilNotNavigatingAnymoreAsync();
                }

                _GuiAndAppHandler.IndicateBusy(true);

                if (oucidAggregatedResponse.MarkupValidation) {
                    if (!await DoWebClientValidationAsync()) {
                        _Model.Status.Text = string.Format(Properties.Resources.OucidResponseShouldNotSayMarkupValidation, url);
                        _Model.Status.Type = StatusType.Error;
                        return;
                    }
                    _SimpleLogger.LogInformationWithCallStack("App validating markup", methodNamesFromStack);
                    try {
                        var validationResult = await _SecuredHttpGate.IsHtmlMarkupValidAsync(_Model.WebViewContentSource.Text);
                        if (!validationResult.Success) {
                            _SimpleLogger.LogInformationWithCallStack(Properties.Resources.MarkupValidationFailed, methodNamesFromStack);
                            _Model.Status.Text = validationResult.ErrorMessage;
                            _Model.Status.Type = StatusType.Error;
                            return;
                        }
                    } catch {
                        _SimpleLogger.LogInformationWithCallStack(Properties.Resources.MarkupValidationFailed, methodNamesFromStack);
                        _Model.Status.Text = Properties.Resources.MarkupValidationFailed;
                        _Model.Status.Type = StatusType.Error;
                        return;
                    }
                }
            }

            if (_JustInvoke) {
                using var client = new HttpClient();
                try {
                    string markup;
                    using (var response = await client.GetAsync(_Model.ScriptStepUrl.Text)) {
                        using (var content = response.Content) {
                            markup = await content.ReadAsStringAsync();
                        }
                    }
                    var validationResult = await _SecuredHttpGate.IsHtmlMarkupValidAsync(markup);
                    if (!validationResult.Success) {
                        _SimpleLogger.LogInformationWithCallStack(Properties.Resources.MarkupValidationFailed, methodNamesFromStack);
                        _Model.Status.Text = validationResult.ErrorMessage;
                        _Model.Status.Type = StatusType.Error;
                        return;
                    }
                } catch {
                    _SimpleLogger.LogInformationWithCallStack(Properties.Resources.MarkupValidationFailed, methodNamesFromStack);
                    _Model.Status.Text = Properties.Resources.MarkupValidationFailed;
                    _Model.Status.Type = StatusType.Error;
                    return;
                }
            }

            _WampLogScanner.WaitUntilLogFolderIsStable(startOfExecutionTimeStamp, out errorMessage);
            if (errorMessage != "") {
                _SimpleLogger.LogInformationWithCallStack("Error in wamp log", methodNamesFromStack);
                _Model.Status.Text = errorMessage;
                _Model.Status.Type = StatusType.Error;
                return;
            }

            _SimpleLogger.LogInformationWithCallStack($"{_Model.ScriptStepUrl.Text} loaded and validated", methodNamesFromStack);

            _Model.Status.Text = "";
            _Model.Status.Type = StatusType.None;
        }
    }

    private async Task<bool> DoWebClientValidationAsync() {
        var secretDenyWebClientValidationCriteria = new DenyWebClientValidationCriteriaSecret();
        var errorsAndInfos = new ErrorsAndInfos();
        var criteria = await _SecretRepository.GetAsync(secretDenyWebClientValidationCriteria, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            return true;
        }

        return !criteria.Any(c => _Model.ScriptStepUrl.Text.Contains(c.UrlPart));
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        _Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.FormOrControlOrIdOrClassTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await Task.FromResult(new List<Selectable>());
    }
}