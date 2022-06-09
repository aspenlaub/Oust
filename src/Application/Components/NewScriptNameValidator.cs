using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.EntityFrameworkCore;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;

public class NewScriptNameValidator : INewScriptNameValidator {
    private readonly IContextFactory _ContextFactory;

    public NewScriptNameValidator(IContextFactory contextFactory) {
        _ContextFactory = contextFactory;
    }

    public async Task<bool> IsNewScriptNameValidAsync(EnvironmentType environmentType, string newScriptName) {
        if (string.IsNullOrWhiteSpace(newScriptName)) { return false; }
        if (newScriptName.Length < 8) { return false; }
        if (new[] { "?", ":", "\\", "*", "%", "/", "|", "\"", "<", ">" }.Any(newScriptName.Contains)) { return false; }

        return !await IsNewScriptNameInUseAsync(environmentType, newScriptName);
    }

    private async Task<bool> IsNewScriptNameInUseAsync(EnvironmentType environmentType, string newScriptName) {
        await using var context = await _ContextFactory.CreateAsync(environmentType);
        var script = await context.Scripts.FirstOrDefaultAsync(s => s.Name == newScriptName);
        return script != null;
    }
}