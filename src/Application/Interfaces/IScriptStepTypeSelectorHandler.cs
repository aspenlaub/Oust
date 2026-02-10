using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IScriptStepTypeSelectorHandler {
    void UpdateSelectableScriptStepTypes();
    Task ScriptStepTypeSelectedIndexChangedAsync(int scriptStepTypeSelectedIndex, bool clearStepDetails);
}