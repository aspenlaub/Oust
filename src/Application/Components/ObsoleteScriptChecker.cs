using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;

public class ObsoleteScriptChecker : IObsoleteScriptChecker {
    private readonly IApplicationModel _Model;
    private readonly IList<string> _StartingWords = new List<string> {
        "and", "but", "given", "then", "when"
    };
    private readonly IContextFactory _ContextFactory;

    public ObsoleteScriptChecker(IApplicationModel model, IContextFactory contextFactory) {
        _Model = model;
        _ContextFactory = contextFactory;
    }

    public async Task<bool> IsTopLevelScriptObsoleteAsync(string name) {
        if (!_StartingWords.Any(w => name.StartsWith(w, StringComparison.InvariantCultureIgnoreCase))) { return false; }

        await using var context = await _ContextFactory.CreateAsync(_Model.EnvironmentType);
        var scriptSteps = await context.ScriptSteps.Where(s => s.SubScriptName == name).ToListAsync();
        return scriptSteps.Any();
    }
}