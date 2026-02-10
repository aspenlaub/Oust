using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class ScriptSelectorHandler : IScriptSelectorHandler {
    private readonly IApplicationModel _Model;
    private readonly IScriptStepSelectorHandler _ScriptStepSelectorHandler;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;
    private readonly IContextFactory _ContextFactory;

    public ScriptSelectorHandler(IApplicationModel model, IScriptStepSelectorHandler scriptStepSelectorHandler, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IContextFactory contextFactory) {
        _Model = model;
        _GuiAndAppHandler = guiAndAppHandler;
        _ScriptStepSelectorHandler = scriptStepSelectorHandler;
        _ContextFactory = contextFactory;
    }

    public async Task EnsureNewScriptAsync() {
        await using var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType);
        var script = await context.Scripts.FirstOrDefaultAsync(s => s.Name == Script.NewScriptName);
        if (script != null) { return; }

        script = new Script { Name = Script.NewScriptName };
        await context.AddAsync(script);
        await context.SaveChangesAsync();
    }

    public async Task UpdateSelectableScriptsAsync() {
        List<Script> scripts;
        await using (var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType)) {
            scripts = await context.Scripts.OrderBy(s => s.Name).ToListAsync();
        }

        var selectables = scripts.Select(s => new Selectable { Guid = s.Guid, Name = s.Name }).ToList();
        if (_Model.SelectedScript.AreSelectablesIdentical(selectables)) { return; }

        _Model.SelectedScript.UpdateSelectables(selectables);
        if (_Model.SelectedScript.SelectedIndex >= 0) { return; }

        await SelectedScriptSelectedIndexChangedAsync(_Model.SelectedScript.Selectables.FindIndex(s => s.Name == Script.NewScriptName), true);
    }

    public async Task SelectedScriptSelectedIndexChangedAsync(int selectedScriptSelectedIndex, bool resetExecutionStack) {
        if (_Model.SelectedScript.SelectedIndex == selectedScriptSelectedIndex) { return; }

        _Model.SelectedScript.SelectedIndex = selectedScriptSelectedIndex;

        if (resetExecutionStack) {
            _Model.ExecutionStackItems.Clear();
        }

        if (_Model.SelectedScript.SelectedIndex >= 0) {
            await using var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType);
            var script = await context.LoadScriptWithStepsAsync(_Model.SelectedScript.SelectedItem.Guid);
            if (script.ProperlyEndReturnIfChanged()) {
                await context.SaveChangesAsync();
            }
        }

        await _ScriptStepSelectorHandler.UpdateSelectableScriptStepsAsync();
        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }
}