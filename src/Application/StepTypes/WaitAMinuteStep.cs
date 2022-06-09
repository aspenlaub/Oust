using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class WaitAMinuteStep : IScriptStepLogic {
    private readonly IApplicationModel _Model;

    public WaitAMinuteStep(IApplicationModel model) {
        _Model = model;
    }

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        return Task.FromResult(true);
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep { ScriptStepType = ScriptStepType.WaitAMinute };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        await Task.Delay(TimeSpan.FromSeconds(65));
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        _Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.FormOrControlOrIdOrClassTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await Task.FromResult(new List<Selectable>());
    }
}