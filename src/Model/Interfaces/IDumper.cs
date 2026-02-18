using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;

public interface IDumper {
    Task DumpAsync(IErrorsAndInfos errorsAndInfos);
    Task<bool> DumpScriptsAsync(IFolder dumpFolder, bool checkXsiXsd);
}