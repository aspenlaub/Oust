namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IApplicationHandlers {
    IFormOrControlOrIdOrClassHandler FormOrControlOrIdOrClassHandler { get; set; }
    IScriptSelectorHandler ScriptSelectorHandler { get; set; }
    IScriptStepSelectorHandler ScriptStepSelectorHandler { get; set; }
    IScriptStepTypeSelectorHandler ScriptStepTypeSelectorHandler { get; set; }
    ISelectedValueSelectorHandler SelectedValueSelectorHandler { get; set; }
    ISubScriptSelectorHandler SubScriptSelectorHandler { get; set; }
}