using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class SelectedValueSelectorHandler : ISelectedValueSelectorHandler {
    private readonly IApplicationModel _Model;
    private readonly IOutrapHelper _OutrapHelper;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;

    public SelectedValueSelectorHandler(IApplicationModel model, IOutrapHelper outrapHelper, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler) {
        _Model = model;
        _OutrapHelper = outrapHelper;
        _GuiAndAppHandler = guiAndAppHandler;
    }

    public async Task EnableOrDisableSelectedValueAsync() {
        await UpdateSelectableSelectedValuesAsync();
    }

    private async Task UpdateSelectableSelectedValuesAsync() {
        var scriptStepType = _Model.ScriptStepType.SelectionMade ? (ScriptStepType)int.Parse(_Model.ScriptStepType.SelectedItem.Guid) : ScriptStepType.EndOfScript;
        var selectables = scriptStepType is ScriptStepType.Select or ScriptStepType.RecognizeSelection
            ? await _OutrapHelper.SelectionChoicesAsync(_Model.WithScriptStepOutrapForm?.Guid, _Model.WithScriptStepOutrapFormInstanceNumber, _Model.FormOrControlOrIdOrClass?.SelectedItem?.Guid)
            : new List<Selectable>();
        _Model.SelectedValue.UpdateSelectables(selectables);
        if (_Model.SelectedValue.SelectionMade) {
            await SelectedValueSelectedIndexChangedAsync(_Model.SelectedValue.SelectedIndex, true);
        } else {
            await SelectedValueSelectedIndexChangedAsync(_Model.SelectedValue.Selectables.Any() ? 0 : -1, true);
        }
    }

    public async Task SelectedValueSelectedIndexChangedAsync(int selectedIndex, bool selectablesChanged) {
        if (!selectablesChanged && _Model.SelectedValue.SelectedIndex == selectedIndex) { return; }

        _Model.SelectedValue.SelectedIndex = selectedIndex;
        await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
    }
}