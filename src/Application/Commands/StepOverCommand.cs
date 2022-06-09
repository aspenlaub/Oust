using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class StepOverCommand : ICommand {
    private readonly IApplicationModel _Model;
    private readonly ICommand _StepIntoCommand;

    public StepOverCommand(IApplicationModel model, ICommand stepIntoCommand) {
        _Model = model;
        _StepIntoCommand = stepIntoCommand;
    }

    public async Task ExecuteAsync() {
        if (!_Model.StepOver.Enabled) { return; }
        if (!_Model.ScriptStepType.SelectionMade) { return; }

        var isSubScriptStep = (int)ScriptStepType.SubScript == int.Parse(_Model.ScriptStepType.SelectedItem.Guid);
        var executionStackSize = _Model.ExecutionStackItems.Count;
        await _StepIntoCommand.ExecuteAsync();
        if (!isSubScriptStep) { return; }

        string currentScriptGuid = "", currentScriptStepGuid = "";
        while (_Model.Status.Type != StatusType.Error && _Model.StepInto.Enabled && executionStackSize < _Model.ExecutionStackItems.Count) {
            if (currentScriptGuid == _Model.SelectedScript.SelectedItem?.Guid && currentScriptStepGuid == _Model.ScriptSteps.SelectedItem?.Guid) {
                _Model.Status.Text = Properties.Resources.ErrorSteppingOver;
                _Model.Status.Type = StatusType.Error;
                return;
            }
            currentScriptGuid = _Model.SelectedScript.SelectedItem?.Guid;
            currentScriptStepGuid = _Model.ScriptSteps.SelectedItem?.Guid;
            await _StepIntoCommand.ExecuteAsync();
        }
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await _StepIntoCommand.ShouldBeEnabledAsync();
    }
}