using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class DuplicateCommand : ICommand {
    private readonly IScriptSelectorHandler _ScriptSelectorHandler;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;
    private readonly IApplicationModel _Model;
    private readonly INewScriptNameValidator _NewScriptNameValidator;
    private readonly IContextFactory _ContextFactory;

    public DuplicateCommand(IApplicationModel model, IScriptSelectorHandler scriptSelectorHandler, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, INewScriptNameValidator newScriptNameValidator, IContextFactory contextFactory) {
        _Model = model;
        _ScriptSelectorHandler = scriptSelectorHandler;
        _GuiAndAppHandler = guiAndAppHandler;
        _NewScriptNameValidator = newScriptNameValidator;
        _ContextFactory = contextFactory;
    }

    public async Task ExecuteAsync() {
        if (!_Model.DuplicateScript.Enabled) { return; }

        var guid = Guid.NewGuid().ToString();
        await using (var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType)) {
            var script = await context.Scripts.Include(s => s.ScriptSteps).FirstOrDefaultAsync(s => s.Guid == _Model.SelectedScript.SelectedItem.Guid);
            if (script == null) { return; }

            var duplicate = script.Duplicate(guid, _Model.NewScriptName.Text);
            await context.AddAsync(duplicate);
            await context.SaveChangesAsync();

            _Model.NewScriptName.Text = "";
        }

        await _ScriptSelectorHandler.UpdateSelectableScriptsAsync();
        var selectedIndex = _Model.SelectedScript.Selectables.FindIndex(s => s.Guid == guid);
        if (selectedIndex >= 0) {
            await _ScriptSelectorHandler.SelectedScriptSelectedIndexChangedAsync(selectedIndex, true);
        }
        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        if (!await _NewScriptNameValidator.IsNewScriptNameValidAsync(_Model.EnvironmentType, _Model.NewScriptName.Text)) { return false; }

        if (_Model.SelectedScript.SelectedIndex < 0) { return false; }
        return _Model.SelectedScript.SelectedItem.Name != _Model.NewScriptName.Text
            && _Model.SelectedScript.SelectedItem.Name != Script.NewScriptName;
    }
}
