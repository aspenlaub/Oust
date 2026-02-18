using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;

public interface IDumpFolderChecker {
    Task CheckDumpFolderAsync(IErrorsAndInfos errorsAndInfos);
}