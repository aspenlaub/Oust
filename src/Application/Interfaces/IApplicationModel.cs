using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Controls;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

// ReSharper disable UnusedMemberInSuper.Global

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IApplicationModel : IWebViewApplicationModelBase {
    EnvironmentType EnvironmentType { get; }

    ISelector SelectedScript { get; }
    ISelector ScriptStepType { get; }
    ISelector FormOrControlOrIdOrClass { get; }
    ISelector SelectedValue { get; }
    ISelector SubScript { get; }
    ISelector ScriptSteps { get; }

    ITextBox NewScriptName { get; }
    ITextBox FormOrIdOrClassInstanceNumber { get; }
    ITextBox FreeText { get; }
    ITextBox WebViewCheckBoxesChecked { get; }
    ITextBox WebViewParagraphs { get; }
    ITextBox WebViewInputValues { get; }
    ITextBox WebViewSelectedValues { get; }
    ITextBox ProcessId { get; }
    ITextBox ProcessBusy { get; }
    ITextBox StatusConfirmedAt { get; }
    ITextBox CurrentTaskType { get; }
    ITextBox CurrentTaskControl { get; }
    ITextBox CurrentTaskState { get; }
    ITextBox ScriptStepUrl { get; }
    ITextBox ExpectedContents { get; }
    ITextBox ScriptStepInput { get; }

    Button RenameScript { get; }
    Button DuplicateScript { get; }
    Button AddOrReplaceStep { get; }
    Button StepOver { get; }
    Button StepInto { get; }
    Button Play { get; }
    Button MoveUp { get; }
    Button Delete { get; }
    Button CodeCoverage { get; }
    Button StopCodeCoverage { get; }
    Button ExtractSubScript { get; }
    Button Consolidate { get; }
    Button ShowExecutionStack { get; }

    Selectable ScriptStepOutrapForm { get; set; }
    Selectable WithScriptStepOutrapForm { get; set; }
    int WithScriptStepOutrapFormInstanceNumber { get; set; }

    Selectable ScriptStepOutOfControl { get; set; }
    Selectable WithScriptStepOutOfControl { get; set; }

    string ScriptStepIdOrClass { get; set; }
    string WithScriptStepIdOrClass { get; set; }
    int WithScriptStepIdOrClassInstanceNumber { get; set; }

    Stack<IExecutionStackItem> ExecutionStackItems { get; }
}