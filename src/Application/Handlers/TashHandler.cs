using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Enums;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Handlers;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class TashHandler : TashHandlerBase<ApplicationModel> {
    public TashHandler(ITashAccessor tashAccessor, ISimpleLogger simpleLogger, IButtonNameToCommandMapper buttonNameToCommandMapper,
            IToggleButtonNameToHandlerMapper toggleButtonNameToHandlerMapper, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler,
            ITashVerifyAndSetHandler<ApplicationModel> tashVerifyAndSetHandler, ITashSelectorHandler<ApplicationModel> tashSelectorHandler,
            ITashCommunicator<ApplicationModel> tashCommunicator, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor)
        : base(tashAccessor, simpleLogger, buttonNameToCommandMapper, toggleButtonNameToHandlerMapper, guiAndAppHandler, tashVerifyAndSetHandler,
                tashSelectorHandler, tashCommunicator, methodNamesFromStackFramesExtractor) {
    }

    protected override void OnStatusChangedToProcessingCommunicated(ITashTaskHandlingStatus<ApplicationModel> status) {
        if (status.TaskBeingProcessed.Type != ControllableProcessTaskType.Reset) { return; }

        status.TaskBeingProcessed.ControlName = nameof(status.Model.SelectedScript);
        status.TaskBeingProcessed.Text = Script.NewScriptName;
    }

    protected override async Task ProcessSingleTaskAsync(ITashTaskHandlingStatus<ApplicationModel> status) {
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(TashAccessor)))) {
            var methodNamesFromStack = MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
            SimpleLogger.LogInformationWithCallStack($"Processing a task of type {status.TaskBeingProcessed.Type} in {nameof(TashHandler)}", methodNamesFromStack);

            switch (status.TaskBeingProcessed.Type) {
                case ControllableProcessTaskType.Reset:
                    await TashSelectorHandler.ProcessSelectComboOrResetTaskAsync(status);
                    break;
                case ControllableProcessTaskType.VerifyIntegrationTestEnvironment:
                    await ProcessVerifyIntegrationTestEnvironmentTaskAsync(status);
                    break;
                default:
                    await base.ProcessSingleTaskAsync(status);
                    break;
            }
        }
    }

    private async Task ProcessVerifyIntegrationTestEnvironmentTaskAsync(ITashTaskHandlingStatus<ApplicationModel> status) {
        if (status.Model.EnvironmentType == EnvironmentType.UnitTest) {
            status.Model.Status.Type = StatusType.Success;
            status.Model.Status.Text = "";
        } else {
            status.Model.Status.Type = StatusType.Error;
            status.Model.Status.Text = $"Environment type is {Enum.GetName(typeof(EnvironmentType), status.Model.EnvironmentType)}";
        }
        await TashCommunicator.CommunicateAndShowCompletedOrFailedAsync(status, false, "");
    }
}