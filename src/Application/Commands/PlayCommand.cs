using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class PlayCommand(IApplicationModel model, ICommand stepOverCommand) : ICommand {
    public async Task ExecuteAsync() {
        if (!model.Play.Enabled) { return; }

        int selectedScriptStepIndex;
        do {
            selectedScriptStepIndex = model.ScriptSteps.SelectedIndex;
            await stepOverCommand.ExecuteAsync();
        } while (selectedScriptStepIndex < model.ScriptSteps.SelectedIndex && model.Status.Type != StatusType.Error && model.StepOver.Enabled);

        if (model.Status.Type == StatusType.Error) { return; }
        if (selectedScriptStepIndex + 1 < model.ScriptSteps.SelectedIndex) { return; }
        if (model.ExecutionStackItems.Any()) { return; }
        if (model.ScriptStepType.SelectedItem.Guid != ((int)ScriptStepType.EndOfScript).ToString()) { return; }

        model.Status.Text = Properties.Resources.ScriptExecutedWithoutErrors;
        model.Status.Type = StatusType.Success;
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await stepOverCommand.ShouldBeEnabledAsync();
    }
}