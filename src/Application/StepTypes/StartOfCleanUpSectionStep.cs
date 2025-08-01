using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

internal class StartOfCleanUpSectionStep(IApplicationModel model) : IScriptStepLogic {
    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        return Task.FromResult(true);
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep { ScriptStepType = ScriptStepType.StartOfCleanUpSection };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        await Task.CompletedTask;
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.FormOrControlOrIdOrClassTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await Task.FromResult(new List<Selectable>());
    }
}