using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;

public interface IImporter {
    bool AnythingToImport(string folder);
    Task<ImportAFileResult> ImportAFileAsync(string inFolder, string doneFolder, IList<string> fileNamesToExclude);
    Task ImportAsync(IErrorsAndInfos errorsAndInfos);
}