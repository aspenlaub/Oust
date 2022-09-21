using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Controls;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application;

public class ApplicationModel : WebViewApplicationModelBase, IApplicationModel {
    public ApplicationModel(EnvironmentType environmentType) {
        EnvironmentType = environmentType;
    }

    public EnvironmentType EnvironmentType { get; }

    public ISelector SelectedScript { get; } = new ComboBox();
    public ISelector ScriptStepType { get; } = new ComboBox();
    public ISelector FormOrControlOrIdOrClass { get; } = new ComboBox();
    public ISelector SelectedValue { get; } = new ComboBox();
    public ISelector SubScript { get; } = new ComboBox();
    public ISelector ScriptSteps { get; } = new ListBox();

    public ITextBox NewScriptName { get; } = new TextBox();
    public ITextBox FormOrIdOrClassInstanceNumber { get; } = new TextBox();
    public ITextBox FreeText { get; } = new TextBox();
    public ITextBox WebViewCheckBoxesChecked { get; } = new TextBox();
    public ITextBox WebViewParagraphs { get; } = new TextBox();
    public ITextBox WebViewInputValues { get; } = new TextBox();
    public ITextBox WebViewSelectedValues { get; } = new TextBox();
    public ITextBox ProcessId { get; } = new TextBox();
    public ITextBox ProcessBusy { get; } = new TextBox();
    public ITextBox StatusConfirmedAt { get; } = new TextBox();
    public ITextBox CurrentTaskType { get; } = new TextBox();
    public ITextBox CurrentTaskControl { get; } = new TextBox();
    public ITextBox CurrentTaskState { get; } = new TextBox();

    public Button RenameScript { get; } = new();
    public Button DuplicateScript { get; } = new();
    public Button AddOrReplaceStep { get; } = new();
    public Button StepOver { get; } = new();
    public Button StepInto { get; } = new();
    public Button Play { get; } = new();
    public Button MoveUp { get; } = new();
    public Button Delete { get; } = new();
    public Button CodeCoverage { get; } = new();
    public Button StopCodeCoverage { get; } = new();
    public Button ExtractSubScript { get; } = new();
    public Button Consolidate { get; } = new();
    public Button ShowExecutionStack { get; } = new();

    public ITextBox ScriptStepUrl => FreeText;
    public ITextBox ExpectedContents => FreeText;
    public ITextBox ScriptStepInput => FreeText;

    public Selectable ScriptStepOutrapForm { get; set; }
    public Selectable WithScriptStepOutrapForm { get; set; }
    public int WithScriptStepOutrapFormInstanceNumber { get; set; }

    public Selectable ScriptStepOutOfControl { get; set; }
    public Selectable WithScriptStepOutOfControl { get; set; }

    public string ScriptStepIdOrClass { get; set; }
    public string WithScriptStepIdOrClass { get; set; } = "";
    public int WithScriptStepIdOrClassInstanceNumber { get; set; }

    public Stack<IExecutionStackItem> ExecutionStackItems { get; } = new();
}