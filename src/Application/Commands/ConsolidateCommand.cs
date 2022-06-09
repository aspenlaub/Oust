using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class ConsolidateCommand : ICommand {
    private readonly IApplicationModel _Model;
    private readonly IProgressWindow _ProgressWindow;
    private readonly IScriptAndSubScriptsConsolidator _Consolidator;

    public ConsolidateCommand(IApplicationModel model, IScriptAndSubScriptsConsolidator consolidator, IProgressWindow progressWindow) {
        _Model = model;
        _Consolidator = consolidator;
        _ProgressWindow = progressWindow;
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        if (!_Model.Consolidate.Enabled) { return; }

        _ProgressWindow.Show(Properties.Resources.ConsolidateProgressCaption);
        _ProgressWindow.AddMessage(Properties.Resources.RetrievingSubScripts);
        var subScripts = await _Consolidator.GetSubScriptsOrderedByDescendingSizeAsync(_Model.EnvironmentType);
        for (var i = 0; i < subScripts.Count; i ++) {
            if (i % 10 == 0) {
                _ProgressWindow.AddMessage(string.Format(Properties.Resources.ProcessingSubScripts, i + 1, Math.Min(i + 9, subScripts.Count), subScripts.Count));
            }
            var scripts = await _Consolidator.GetScriptsForWhichSubScriptCouldBeUsedAsync(_Model.EnvironmentType, subScripts[i]);
            foreach (var script in scripts) {
                _ProgressWindow.AddMessage("    " + string.Format(Properties.Resources.SubScriptCouldBeUsedInScript, subScripts[i].Name, script.Name));
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        _ProgressWindow.AddMessage(Properties.Resources.Done);
    }
}