using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class EndOfScriptStep : IScriptStepLogic {
    private readonly IApplicationModel _Model;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public EndOfScriptStep(IApplicationModel model) {
        _Model = model;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        return await Task.FromResult(true);
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep { ScriptStepType = ScriptStepType.EndOfScript };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        await Task.CompletedTask;

        _Model.Status.Text = "";
        _Model.Status.Type = StatusType.None;
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        _Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.FormOrControlOrIdOrClassTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await Task.FromResult(new List<Selectable>());
    }
}