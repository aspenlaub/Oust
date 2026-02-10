using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IScriptStepSelectorHandler {
    Task UpdateSelectableScriptStepsAsync();
    Task UpdateSelectableScriptStepsAfterCurrentHasBeenAddedAsync();
    Task UpdateSelectableScriptStepsAfterCurrentHasBeenMovedUpAsync();
    Task UpdateSelectableScriptStepsAfterCurrentHasBeenDeletedAsync();
    Task ScriptStepsSelectedIndexChangedAsync(int scriptStepsSelectedIndex, bool selectablesChanged);
}