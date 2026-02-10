using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface INewScriptNameValidator {
    Task<bool> IsNewScriptNameValidAsync(EnvironmentType environmentType, string newScriptName);
}