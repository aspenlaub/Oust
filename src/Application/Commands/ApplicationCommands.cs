using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class ApplicationCommands : IApplicationCommands {
    public ICommand AddOrReplaceStepCommand { get; set; }
    public ICommand CodeCoverageCommand { get; set; }
    public ICommand ConsolidateCommand { get; set; }
    public ICommand DeleteStepCommand { get; set; }
    public ICommand ExtractSubScriptCommand { get; set; }
    public ICommand MoveUpStepCommand { get; set; }
    public ICommand PlayCommand { get; set; }
    public ICommand RenameCommand { get; set; }
    public ICommand DuplicateCommand { get; set; }
    public ICommand ShowExecutionStackCommand { get; set; }
    public ICommand StepIntoCommand { get; set; }
    public ICommand StepOverCommand { get; set; }
    public ICommand StopCodeCoverageCommand { get; set; }
    public ICommand SelectScriptFromListCommand { get; set; }
}