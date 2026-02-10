using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using ModelResources = Aspenlaub.Net.GitHub.CSharp.Oust.Model.Properties.Resources;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class RecoverCommand(IApplicationModel model, ICommand stepOverCommand, IScriptStepSelectorHandler scriptStepSelectorHandler)
        : ICommand {
    public async Task ExecuteAsync() {
        if (!model.Recover.Enabled) { return; }

        int selectedScriptStepIndex;
        for (selectedScriptStepIndex = 0; selectedScriptStepIndex < model.ScriptSteps.Selectables.Count
             && !model.ScriptSteps.Selectables[selectedScriptStepIndex].Name.StartsWith(ModelResources.StartOfCleanUpSection); selectedScriptStepIndex ++) {
        }
        if (selectedScriptStepIndex >= model.ScriptSteps.Selectables.Count) {
            model.Status.Text = Properties.Resources.NoStepOfTypeStartOfCleanUpSection;
            model.Status.Type = StatusType.None;
            return;
        }
        await scriptStepSelectorHandler.ScriptStepsSelectedIndexChangedAsync(selectedScriptStepIndex, false);
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