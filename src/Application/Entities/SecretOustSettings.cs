using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;

public class SecretOustSettings : ISecret<OustSettings> {

    private OustSettings _DefaultOustSettings;
    public OustSettings DefaultValue => _DefaultOustSettings ??= new OustSettings {
        new() { Machine = Environment.MachineName, ShouldWindows11BeAssumed = false }
    };
    public string Guid => "A7DB0DF7-B576-4310-BE2F-A826D09781A1";
}