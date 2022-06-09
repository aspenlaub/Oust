using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class ApplicationHandlers : IApplicationHandlers {
    public IFormOrControlOrIdOrClassHandler FormOrControlOrIdOrClassHandler { get; set; }
    public IScriptSelectorHandler ScriptSelectorHandler { get; set; }
    public IScriptStepSelectorHandler ScriptStepSelectorHandler { get; set; }
    public IScriptStepTypeSelectorHandler ScriptStepTypeSelectorHandler { get; set; }
    public ISelectedValueSelectorHandler SelectedValueSelectorHandler { get; set; }
    public ISubScriptSelectorHandler SubScriptSelectorHandler { get; set; }
}