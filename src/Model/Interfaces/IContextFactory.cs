using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;

public interface IContextFactory {
    Task<Context> CreateAsync(EnvironmentType environmentType);
    Task<Context> CreateAsync(EnvironmentType environmentType, SynchronizationContext synchronizationContext);
}