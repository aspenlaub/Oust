using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IScriptStepLogic {
    Task<bool> CanBeAddedOrReplaceExistingStepAsync();
    IScriptStep CreateScriptStepToAdd();
    Task<bool> ShouldBeEnabledAsync();
    Task ExecuteAsync();
    void SetFormOrControlOrIdOrClassTitle();
    Task<IList<Selectable>> SelectableFormsOrControlsOrIdsOrClassesAsync();
    string FreeCodeLabelText { get; }
}