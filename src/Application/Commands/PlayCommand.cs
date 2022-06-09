using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class PlayCommand : ICommand {
    private readonly IApplicationModel _Model;
    private readonly ICommand _StepOverCommand;

    public PlayCommand(IApplicationModel model, ICommand stepOverCommand) {
        _Model = model;
        _StepOverCommand = stepOverCommand;
    }

    public async Task ExecuteAsync() {
        if (!_Model.Play.Enabled) { return; }

        int selectedScriptStepIndex;
        do {
            selectedScriptStepIndex = _Model.ScriptSteps.SelectedIndex;
            await _StepOverCommand.ExecuteAsync();
        } while (selectedScriptStepIndex < _Model.ScriptSteps.SelectedIndex && _Model.Status.Type != StatusType.Error && _Model.StepOver.Enabled);

        if (_Model.Status.Type == StatusType.Error) { return; }
        if (selectedScriptStepIndex + 1 < _Model.ScriptSteps.SelectedIndex) { return; }
        if (_Model.ExecutionStackItems.Any()) { return; }
        if (_Model.ScriptStepType.SelectedItem.Guid != ((int)ScriptStepType.EndOfScript).ToString()) { return; }

        _Model.Status.Text = Properties.Resources.ScriptExecutedWithoutErrors;
        _Model.Status.Type = StatusType.Success;
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await _StepOverCommand.ShouldBeEnabledAsync();
    }
}