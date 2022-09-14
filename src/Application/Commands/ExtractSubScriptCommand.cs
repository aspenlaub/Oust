using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class ExtractSubScriptCommand : ICommand {
    private readonly IApplicationModel _Model;
    private readonly IExtractSubScriptPopup _Popup;
    private readonly ISubScriptExtractor _SubScriptExtractor;
    private readonly IScriptSelectorHandler _ScriptSelectorHandler;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;

    public ExtractSubScriptCommand(IApplicationModel model, IExtractSubScriptPopup popup, ISubScriptExtractor subScriptExtractor,
        IScriptSelectorHandler scriptSelectorHandler, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler) {
        _Model = model;
        _Popup = popup;
        _SubScriptExtractor = subScriptExtractor;
        _ScriptSelectorHandler = scriptSelectorHandler;
        _GuiAndAppHandler = guiAndAppHandler;
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(_Model.ScriptSteps.Selectables.Count >= 3);
    }

    public async Task ExecuteAsync() {
        _Model.IsBusy = false;

        if (!_Model.ExtractSubScript.Enabled) { return; }

        var extractSubScriptSpecification = _Popup.Show(_Model.ScriptSteps);
        if (extractSubScriptSpecification?.StepsToExtract.Any() != true) { return; }

        if (await _SubScriptExtractor.ExtractSubScriptAsync(_Model.EnvironmentType, _Model.SelectedScript.SelectedItem.Guid, extractSubScriptSpecification)) {
            await _ScriptSelectorHandler.UpdateSelectableScriptsAsync();
            var index = _Model.SelectedScript.SelectedIndex;
            _Model.SelectedScript.SelectedIndex = -1;
            await _ScriptSelectorHandler.SelectedScriptSelectedIndexChangedAsync(index, true);
            await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
        } else {
            _Model.Status.Text = Properties.Resources.CouldNotExtractSubScript;
            _Model.Status.Type = StatusType.Error;
        }
    }
}