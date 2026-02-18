using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Helpers;

public class OustSettingsHelper : IOustSettingsHelper {
    private readonly ISecretRepository _SecretRepository;

    public OustSettingsHelper(ISecretRepository secretRepository) {
        _SecretRepository = secretRepository;
    }

    public async Task<YesNoInconclusive> ShouldWindows11BeAssumedAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        return await ShouldWindows11BeAssumedAsync(errorsAndInfos);
    }

    public async Task<YesNoInconclusive> ShouldWindows11BeAssumedAsync(IErrorsAndInfos errorsAndInfos) {
        var secret = new SecretOustSettings();
        var settings = await _SecretRepository.GetAsync(secret, errorsAndInfos);
        return errorsAndInfos.AnyErrors()
            ? new YesNoInconclusive { Inconclusive = true }
            : settings.ShouldWindows11BeAssumed();
    }
}