using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class SelectScriptFromListCommand : ICommand {
    private readonly IApplicationModel _Model;
    private readonly ISelectScriptFromListPopupFactory _PopupFactory;
    private readonly IScriptSelectorHandler _Handler;

    public SelectScriptFromListCommand(IApplicationModel model, ISelectScriptFromListPopupFactory popupFactory, IScriptSelectorHandler handler) {
        _Model = model;
        _PopupFactory = popupFactory;
        _Handler = handler;
    }

    public async Task ExecuteAsync() {
        _Model.IsBusy = false;

        var selectableScriptNames = _Model.SelectedScript.Selectables.Select(x => x.Name).OrderBy(x => x).ToList();
        var scriptName = _PopupFactory.Create().ShowDialog(selectableScriptNames);
        if (string.IsNullOrEmpty(scriptName)) {
            return;
        }

        var selectedIndex = _Model.SelectedScript.Selectables.FindIndex(s => s.Name == scriptName);
        if (selectedIndex < 0) {
            return;
        }

        await _Handler.SelectedScriptSelectedIndexChangedAsync(selectedIndex, true);
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(_Model.SelectedScript.Selectables.Any());
    }
}