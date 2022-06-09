using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;

public interface IDumpFolderChecker {
    Task CheckDumpFolderAsync(IErrorsAndInfos errorsAndInfos);
}