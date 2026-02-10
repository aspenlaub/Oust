using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class SubScriptSelectorHandler : ISubScriptSelectorHandler {
    private readonly IApplicationModel _Model;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;
    private readonly IContextFactory _ContextFactory;

    public SubScriptSelectorHandler(IApplicationModel model, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, IContextFactory contextFactory) {
        _Model = model;
        _GuiAndAppHandler = guiAndAppHandler;
        _ContextFactory = contextFactory;
    }

    public async Task EnableOrDisableSubScriptAsync() {
        await UpdateSelectableSubScriptsAsync();
    }

    private async Task UpdateSelectableSubScriptsAsync() {
        var scriptStepType = _Model.ScriptStepType.SelectionMade ? (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid) : ScriptStepType.EndOfScript;
        List<Selectable> selectables;
        if (scriptStepType == ScriptStepType.SubScript) {
            List<Script> scripts;
            await using (var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType)) {
                scripts = await context.Scripts.Where(s => s.Name != Script.NewScriptName).OrderBy(s => s.Name).ToListAsync();
            }

            if (scripts.Any()) {
                selectables = new List<Selectable> { new() { Guid = "", Name = "" }};
                selectables.AddRange(scripts.Select(s => new Selectable { Guid = s.Guid, Name = s.Name }).ToList());
            } else {
                selectables = new List<Selectable>();
            }
        } else {
            selectables = new List<Selectable>();
        }
        _Model.SubScript.UpdateSelectables(selectables);
        if (_Model.SubScript.SelectionMade) {
            await SubScriptSelectedIndexChangedAsync(_Model.SelectedValue.SelectedIndex, true);
        } else {
            await SubScriptSelectedIndexChangedAsync(_Model.SelectedValue.Selectables.Any() ? 0 : -1, true);
        }
    }

    public async Task SubScriptSelectedIndexChangedAsync(int selectedIndex, bool selectablesChanged) {
        if (!selectablesChanged && _Model.SubScript.SelectedIndex == selectedIndex) { return; }

        _Model.SubScript.SelectedIndex = selectedIndex;
        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }
}