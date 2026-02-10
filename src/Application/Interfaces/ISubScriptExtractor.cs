using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface ISubScriptExtractor {
    Task<bool> ExtractSubScriptAsync(EnvironmentType environmentType, string scriptGuid, IExtractSubScriptSpecification extractSubScriptSpecification);
}