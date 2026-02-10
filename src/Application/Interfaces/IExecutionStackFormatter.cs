using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IExecutionStackFormatter {
    Task<IList<string>> FormatExecutionStackAsync(EnvironmentType environmentType, string currentScriptStepGuid, IList<IExecutionStackItem> stackItems);
}