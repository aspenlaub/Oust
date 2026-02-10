using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using ModelResources = Aspenlaub.Net.GitHub.CSharp.Oust.Model.Properties.Resources;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class StepIntoCommand : ICommand {
    private readonly IApplicationModel _Model;
    private readonly IDictionary<ScriptStepType, IScriptStepLogic> _ScriptStepLogicDictionary;
    private readonly IScriptStepSelectorHandler _ScriptStepSelectorHandler;
    private readonly IScriptSelectorHandler _ScriptSelectorHandler;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;
    private readonly ISimpleLogger _SimpleLogger;
    private readonly IOutrapHelper _OutrapHelper;
    private readonly IMethodNamesFromStackFramesExtractor _MethodNamesFromStackFramesExtractor;

    public StepIntoCommand(IApplicationModel model, IScriptSelectorHandler scriptSelectorHandler, IScriptStepSelectorHandler scriptStepSelectorHandler, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, ISimpleLogger simpleLogger,
            IDictionary<ScriptStepType, IScriptStepLogic> scriptStepLogicDictionary, IOutrapHelper outrapHelper, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor) {
        _Model = model;
        _ScriptStepLogicDictionary = scriptStepLogicDictionary;
        _ScriptStepSelectorHandler = scriptStepSelectorHandler;
        _ScriptSelectorHandler = scriptSelectorHandler;
        _GuiAndAppHandler = guiAndAppHandler;
        _SimpleLogger = simpleLogger;
        _OutrapHelper = outrapHelper;
        _MethodNamesFromStackFramesExtractor = methodNamesFromStackFramesExtractor;
    }

    public async Task ExecuteAsync() {
        if (!_Model.StepInto.Enabled) { return; }
        if (!_Model.ScriptStepType.SelectionMade) { return; }

        var scriptStepType = (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid);
        if (!_ScriptStepLogicDictionary.ContainsKey(scriptStepType)) { return; }

        switch (scriptStepType) {
            case ScriptStepType.SubScript:
                await PushToExecutionStackAndSelectSubScriptAsync();
                break;
            case ScriptStepType.EndOfScript when _Model.ExecutionStackItems.Any():
                await PopFromExecutionStackAndSelectParentScriptStepAsync();
                break;
            default:
                await ExecuteAccordingToStepLogicAsync(scriptStepType);
                break;
        }

        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }

    private async Task ExecuteAccordingToStepLogicAsync(ScriptStepType scriptStepType) {
        using (_SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(IScriptStepLogic.ExecuteAsync) + scriptStepType))) {
            var methodNamesFromStack = _MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
            if (_ScriptStepLogicDictionary[scriptStepType] == null) {
                _Model.Status.Type = StatusType.Error;
                _Model.Status.Text = Properties.Resources.ScriptStepTypeNotFoundInLogicDictionary;
                return;
            }
            await _ScriptStepLogicDictionary[scriptStepType].ExecuteAsync();

            var dictionary = await _OutrapHelper.AuxiliaryDictionaryAsync();
            _Model.WebViewCheckBoxesChecked.Text = dictionary["WebViewCheckBoxesChecked"];
            _Model.WebViewParagraphs.Text = dictionary["WebViewParagraphs"];
            _Model.WebViewInputValues.Text = dictionary["WebViewInputValues"];
            _Model.WebViewSelectedValues.Text = dictionary["WebViewSelectedValues"];

            if (_Model.Status.Type == StatusType.Error || _Model.ScriptSteps.SelectedIndex + 1 >= _Model.ScriptSteps.Selectables.Count) {
                return;
            }

            var nextStepIndex = _Model.ScriptSteps.SelectedIndex + 1;
            if (scriptStepType == ScriptStepType.EndScriptIfRecognized && _Model.Status.Text == Properties.Resources.ContentsFoundEndScript) {
                nextStepIndex = _Model.ScriptSteps.Selectables.Count - 1;

                if (_Model.ScriptSteps.Selectables[nextStepIndex].Name != ModelResources.EndOfScript) {
                    _Model.Status.Type = StatusType.Error;
                    _Model.Status.Text = Properties.Resources.LastStepIsNotEndOfScript;
                    return;
                }
            }

            _SimpleLogger.LogInformationWithCallStack(Properties.Resources.AdvancingToNextStep, methodNamesFromStack);
            await _ScriptStepSelectorHandler.ScriptStepsSelectedIndexChangedAsync(nextStepIndex, false);
            _SimpleLogger.LogInformationWithCallStack(Properties.Resources.AdvancedToNextStep, methodNamesFromStack);
        }
    }

    private async Task PushToExecutionStackAndSelectSubScriptAsync() {
        _Model.ExecutionStackItems.Push(new ExecutionStackItem(_Model.SelectedScript.SelectedItem.Guid, _Model.SelectedScript.SelectedItem.Name, _Model.ScriptSteps.SelectedItem.Guid, true));

        var selectedIndex = _Model.SelectedScript.Selectables.FindIndex(s => s.Guid == _Model.SubScript.SelectedItem.Guid);
        if (selectedIndex < 0) {
            _Model.Status.Text = string.Format(Properties.Resources.SubScriptNotFound, _Model.SubScript.SelectedItem.Name);
            _Model.Status.Type = StatusType.Error;
            return;
        }

        _Model.Status.Text = "";
        _Model.Status.Type = StatusType.None;
        await _ScriptSelectorHandler.SelectedScriptSelectedIndexChangedAsync(selectedIndex, false);
    }

    private async Task PopFromExecutionStackAndSelectParentScriptStepAsync() {
        var stackItem = _Model.ExecutionStackItems.Pop();

        var selectedIndex = _Model.SelectedScript.Selectables.FindIndex(s => s.Guid == stackItem.ScriptGuid);
        if (selectedIndex < 0) {
            _Model.Status.Text = string.Format(Properties.Resources.CallingScriptNotFound, stackItem.ScriptName);
            _Model.Status.Type = StatusType.Error;
            return;
        }

        _Model.Status.Text = "";
        _Model.Status.Type = StatusType.None;
        await _ScriptSelectorHandler.SelectedScriptSelectedIndexChangedAsync(selectedIndex, false);

        selectedIndex = _Model.ScriptSteps.Selectables.FindIndex(s => s.Guid == stackItem.ScriptStepGuid);
        if (selectedIndex < 0) {
            _Model.Status.Text = string.Format(Properties.Resources.CallingScriptStepNotFound, stackItem.ScriptName);
            _Model.Status.Type = StatusType.Error;
            return;
        }

        if (selectedIndex >= _Model.ScriptSteps.Selectables.Count - 1) { return; }

        await _ScriptStepSelectorHandler.ScriptStepsSelectedIndexChangedAsync(selectedIndex + 1, false);
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        if (!_Model.ScriptStepType.SelectionMade) { return false; }
        if (_Model.SelectedScript.SelectedItem.Name == Script.NewScriptName) { return false; }
        var scriptStepType = (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid);
        if (!_ScriptStepLogicDictionary.ContainsKey(scriptStepType)) { return false; }

        return await _ScriptStepLogicDictionary[scriptStepType].ShouldBeEnabledAsync();
    }
}