using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Handlers;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class TashSelectorHandler : TashSelectorHandlerBase<IApplicationModel> {
    private readonly IApplicationHandlers _OustHandlers;
    private readonly IObsoleteScriptChecker _ObsoleteScriptChecker;

    public TashSelectorHandler(IApplicationHandlers oustHandlers, IObsoleteScriptChecker obsoleteScriptChecker, ISimpleLogger simpleLogger,
                ITashCommunicator<IApplicationModel> tashCommunicator, Dictionary<string, ISelector> selectors,
                IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor)
        : base(simpleLogger, tashCommunicator, selectors, methodNamesFromStackFramesExtractor) {
        _OustHandlers = oustHandlers;
        _ObsoleteScriptChecker = obsoleteScriptChecker;
    }

    public override async Task ProcessSelectComboOrResetTaskAsync(ITashTaskHandlingStatus<IApplicationModel> status) {
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(ProcessSelectComboOrResetTaskAsync)))) {
            ISelector selector;
            string itemToSelect, controlName;
            var methodNamesFromStack = MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
            if (status.TaskBeingProcessed.Type != ControllableProcessTaskType.Reset) {
                SimpleLogger.LogInformationWithCallStack("Task type is not Reset", methodNamesFromStack);
                if (status.TaskBeingProcessed.ControlName == nameof(status.Model.SelectedScript)) {
                    if (await _ObsoleteScriptChecker.IsTopLevelScriptObsoleteAsync(status.TaskBeingProcessed.Text)) {
                        SimpleLogger.LogInformationWithCallStack($"Communicating 'Failed' with text '{Properties.Resources.ScriptIsObsoleteOrSubScript}' to remote controlling process",
                            methodNamesFromStack);
                        await TashCommunicator.ChangeCommunicateAndShowProcessTaskStatusAsync(status, ControllableProcessTaskStatus.Failed, false, "",
                            Properties.Resources.ScriptIsObsoleteOrSubScript);
                        return;
                    }

                    SimpleLogger.LogInformationWithCallStack($"Script '{status.TaskBeingProcessed.Text}' is neither obsolete nor a sub script", methodNamesFromStack);
                }
                else if (!Selectors.ContainsKey(status.TaskBeingProcessed.ControlName)) {
                    var errorMessage = $"Unknown selector control {status.TaskBeingProcessed.ControlName}";
                    SimpleLogger.LogInformationWithCallStack($"Communicating 'BadRequest' to remote controlling process ({errorMessage})", methodNamesFromStack);
                    await TashCommunicator.ChangeCommunicateAndShowProcessTaskStatusAsync(status, ControllableProcessTaskStatus.BadRequest, false, "", errorMessage);
                    return;
                }

                SimpleLogger.LogInformationWithCallStack($"{status.TaskBeingProcessed.ControlName} is a valid selector", methodNamesFromStack);
                selector = Selectors[status.TaskBeingProcessed.ControlName];

                controlName = status.TaskBeingProcessed.ControlName;
                await SelectedIndexChangedAsync(status, controlName, 0, false);
                if (status.TaskBeingProcessed.Status == ControllableProcessTaskStatus.BadRequest) {
                    return;
                }

                itemToSelect = status.TaskBeingProcessed.Text;
            }
            else {
                SimpleLogger.LogInformationWithCallStack("Task type is Reset", methodNamesFromStack);
                controlName = nameof(IApplicationModel.SelectedScript);
                selector = Selectors[controlName];
                itemToSelect = Script.NewScriptName;
            }

            await SelectItemAsync(status, selector, itemToSelect, controlName);
        }
    }


    protected override void OnItemAlreadySelected(ITashTaskHandlingStatus<IApplicationModel> status) {
        if (status.TaskBeingProcessed.Type == ControllableProcessTaskType.Reset) {
            status.Model.ExecutionStackItems.Clear();
        }
    }

    protected override async Task SelectedIndexChangedAsync(ITashTaskHandlingStatus<IApplicationModel> status, string controlName, int selectedIndex, bool selectablesChanged) {
        var methodNamesFromStack = MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
        if (selectedIndex >= 0) {
            SimpleLogger.LogInformationWithCallStack($"Changing selected index for {controlName} to {selectedIndex}", methodNamesFromStack);
            switch (controlName) {
                case nameof(status.Model.SelectedScript):
                    await _OustHandlers.ScriptSelectorHandler.SelectedScriptSelectedIndexChangedAsync(selectedIndex, selectablesChanged);
                    break;
                case nameof(status.Model.ScriptSteps):
                    await _OustHandlers.ScriptStepSelectorHandler.ScriptStepsSelectedIndexChangedAsync(selectedIndex, selectablesChanged);
                    break;
                case nameof(status.Model.ScriptStepType):
                    await _OustHandlers.ScriptStepTypeSelectorHandler.ScriptStepTypeSelectedIndexChangedAsync(selectedIndex, selectablesChanged);
                    break;
                case nameof(status.Model.SubScript):
                    await _OustHandlers.SubScriptSelectorHandler.SubScriptSelectedIndexChangedAsync(selectedIndex, false);
                    break;
                case nameof(status.Model.FormOrControlOrIdOrClass):
                    await _OustHandlers.FormOrControlOrIdOrClassHandler.FormOrControlOrIdOrClassSelectedIndexChangedAsync(selectedIndex, selectablesChanged);
                    break;
                case nameof(status.Model.SelectedValue):
                    await _OustHandlers.SelectedValueSelectorHandler.SelectedValueSelectedIndexChangedAsync(selectedIndex, false);
                    break;
                default:
                    var errorMessage = $"Do not know how to select for {status.TaskBeingProcessed.ControlName}";
                    SimpleLogger.LogInformationWithCallStack($"Communicating 'BadRequest' to remote controlling process ({errorMessage})", methodNamesFromStack);
                    await TashCommunicator.ChangeCommunicateAndShowProcessTaskStatusAsync(status, ControllableProcessTaskStatus.BadRequest, false, "", errorMessage);
                    break;
            }
        }
    }
}