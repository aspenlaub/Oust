using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.StepTypes;

public class SubScriptStep : IScriptStepLogic {
    private readonly IApplicationModel _Model;

    public string FreeCodeLabelText => Properties.Resources.FreeTextTitle;

    public SubScriptStep(IApplicationModel model) {
        _Model = model;
    }

    public async Task<bool> CanBeAddedOrReplaceExistingStepAsync() {
        return await ShouldBeEnabledAsync();
    }

    public IScriptStep CreateScriptStepToAdd() {
        return new ScriptStep {
            ScriptStepType = ScriptStepType.SubScript,
            SubScriptGuid = _Model.SubScript.SelectedItem.Guid,
            SubScriptName = _Model.SubScript.SelectedItem.Name
        };
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(_Model.SubScript.SelectionMade && _Model.SubScript.SelectedIndex > 0);
    }

    public async Task ExecuteAsync() {
        await Task.Run(() => throw new ApplicationException("Sub script step cannot be executed, use step into/over or play"));
    }

    public void SetFormOrControlOrIdOrClassTitle() {
        _Model.FormOrControlOrIdOrClass.LabelText = Properties.Resources.FormOrControlOrIdOrClassTitle;
    }

    public async Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync() {
        return await Task.FromResult(new List<Selectable>());
    }
}