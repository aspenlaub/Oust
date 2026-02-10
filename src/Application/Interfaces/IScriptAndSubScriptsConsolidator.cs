using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IScriptAndSubScriptsConsolidator {
    Task<List<Script>> GetSubScriptsOrderedByDescendingSizeAsync(EnvironmentType environmentType);
    Task<List<Script>> GetScriptsForWhichSubScriptCouldBeUsedAsync(EnvironmentType environmentType, Script subScript);
}