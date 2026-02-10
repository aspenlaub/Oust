using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Handlers;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class TashCommunicator : TashCommunicatorBase<IApplicationModel> {
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndAppHandler;

    public TashCommunicator(ITashAccessor tashAccessor, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, ISimpleLogger simpleLogger, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor)
        : base(tashAccessor, simpleLogger, methodNamesFromStackFramesExtractor) {
        _GuiAndAppHandler = guiAndAppHandler;
    }

    public override async Task ShowStatusAsync(ITashTaskHandlingStatus<IApplicationModel> status) {
        await base.ShowStatusAsync(status);

        var changed = false;
        SetNewTextBoxText(status.Model.ProcessBusy, status.Model.IsBusy ? "Yes" : "No", ref changed);
        SetNewTextBoxText(status.Model.ProcessId, status.ProcessId.ToString(), ref changed);
        SetNewTextBoxText(status.Model.StatusConfirmedAt, status.StatusLastConfirmedAt.Year > 2000 ? status.StatusLastConfirmedAt.ToLongTimeString() : "", ref changed);
        SetNewTextBoxText(status.Model.CurrentTaskType, status.TaskBeingProcessed?.Type ?? "", ref changed);
        SetNewTextBoxText(status.Model.CurrentTaskControl, status.TaskBeingProcessed?.ControlName ?? "", ref changed);
        SetNewTextBoxText(status.Model.CurrentTaskState, status.TaskBeingProcessed?.Status.ToString() ?? "", ref changed);

        if (changed) {
            using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(ShowStatusAsync)))) {
                await _GuiAndAppHandler.EnableOrDisableButtonsThenSyncGuiAndAppAsync();
            }
        }
    }

    private void SetNewTextBoxText(ITextBox textBox, string s, ref bool changed) {
        changed = changed || textBox.Text != s;
        if (!changed) { return; }

        textBox.Text = s;
    }
}