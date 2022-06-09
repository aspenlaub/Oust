using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IOustSettingsHelper {
    Task<YesNoInconclusive> ShouldWindows11BeAssumedAsync();
}