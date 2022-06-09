using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class RenameCommand : ICommand {
    private readonly IScriptSelectorHandler _ScriptSelectorHandler;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;
    private readonly IApplicationModel _Model;
    private readonly INewScriptNameValidator _NewScriptNameValidator;
    private readonly IContextFactory _ContextFactory;

    public RenameCommand(IApplicationModel model, IScriptSelectorHandler scriptSelectorHandler, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, INewScriptNameValidator newScriptNameValidator, IContextFactory contextFactory) {
        _Model = model;
        _ScriptSelectorHandler = scriptSelectorHandler;
        _GuiAndAppHandler = guiAndAppHandler;
        _NewScriptNameValidator = newScriptNameValidator;
        _ContextFactory = contextFactory;
    }

    public async Task ExecuteAsync() {
        if (!_Model.RenameScript.Enabled) { return; }

        await using (var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType)) {
            var script = await context.Scripts.FirstOrDefaultAsync(s => s.Guid == _Model.SelectedScript.SelectedItem.Guid);
            if (script == null) { return; }

            script.Name = _Model.NewScriptName.Text;
            await context.SaveChangesAsync();
            _Model.SelectedScript.SelectedItem.Name = script.Name;
            _Model.NewScriptName.Text = "";
        }

        await _ScriptSelectorHandler.EnsureNewScriptAsync();
        await _ScriptSelectorHandler.UpdateSelectableScriptsAsync();
        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        if (!await _NewScriptNameValidator.IsNewScriptNameValidAsync(_Model.EnvironmentType, _Model.NewScriptName.Text)) { return false; }

        if (_Model.SelectedScript.SelectedIndex < 0) { return false; }
        return _Model.SelectedScript.SelectedItem.Name != _Model.NewScriptName.Text;
    }
}