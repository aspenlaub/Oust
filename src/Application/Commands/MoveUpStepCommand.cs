using System;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class MoveUpStepCommand : ICommand {
    private readonly IApplicationModel _Model;
    private readonly IScriptStepSelectorHandler _ScriptStepSelectorHandler;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;
    private readonly IContextFactory _ContextFactory;

    public MoveUpStepCommand(IApplicationModel model, IScriptStepSelectorHandler scriptStepSelectorHandler, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IContextFactory contextFactory) {
        _Model = model;
        _ScriptStepSelectorHandler = scriptStepSelectorHandler;
        _GuiAndAppHandler = guiAndAppHandler;
        _ContextFactory = contextFactory;
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(ShouldBeEnabled());
    }

    public bool ShouldBeEnabled() {
        if (!_Model.SelectedScript.SelectionMade) { return false; }
        if (_Model.SelectedScript.SelectedItem.Name == Script.NewScriptName) { return false; }
        if (!_Model.ScriptSteps.SelectionMade) { return false; }
        if (_Model.ScriptSteps.SelectedIndex == 0) { return false; }

        var scriptStepType = (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid);
        return scriptStepType != ScriptStepType.EndOfScript;
    }

    public async Task ExecuteAsync() {
        if (!_Model.MoveUp.Enabled) { return; }

        ScriptStepType scriptStepType;

        await using (var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType)) {
            var scriptGuid = _Model.SelectedScript.SelectedItem.Guid;
            var script = await context.LoadScriptWithStepsAsync(scriptGuid);

            var scriptStepToMoveUp = script.ScriptSteps.SingleOrDefault(s => s.Guid == _Model.ScriptSteps.SelectedItem.Guid);
            if (scriptStepToMoveUp == null) { return; }

            scriptStepType = scriptStepToMoveUp.ScriptStepType;

            var precedingSteps = script.ScriptSteps.Where(s => s.StepNumber < scriptStepToMoveUp.StepNumber).ToList();
            if (!precedingSteps.Any()) { return; }

            var exchangeStepNumber = precedingSteps.Max(s => s.StepNumber);
            var exchangeStep = precedingSteps.SingleOrDefault(s => s.StepNumber == exchangeStepNumber);
            if (exchangeStep == null) { return; }

            exchangeStep.StepNumber = scriptStepToMoveUp.StepNumber;
            scriptStepToMoveUp.StepNumber = exchangeStepNumber;

            context.SaveChanges();
        }

        await _ScriptStepSelectorHandler.UpdateSelectableScriptStepsAfterCurrentHasBeenMovedUpAsync();
        if (scriptStepType != (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid)) {
            throw new Exception("Unexpected script step type");
        }
        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }
}
