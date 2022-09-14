using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IApplicationCommands {
    ICommand AddOrReplaceStepCommand { get; set; }
    ICommand CodeCoverageCommand { get; set; }
    ICommand ConsolidateCommand { get; set; }
    ICommand DeleteStepCommand { get; set; }
    ICommand ExtractSubScriptCommand { get; set; }
    ICommand MoveUpStepCommand { get; set; }
    ICommand PlayCommand { get; set; }
    ICommand ShowExecutionStackCommand { get; set; }
    ICommand RenameCommand { get; set; }
    ICommand DuplicateCommand { get; set; }
    ICommand StepIntoCommand { get; set; }
    ICommand StepOverCommand { get; set; }
    ICommand StopCodeCoverageCommand { get; set; }
    ICommand SelectScriptFromListCommand { get; set; }
}