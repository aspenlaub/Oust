using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Enums;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Handlers;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;

public class TashVerifyAndSetHandler : TashVerifyAndSetHandlerBase<IApplicationModel> {
    public TashVerifyAndSetHandler(ISimpleLogger simpleLogger, ITashSelectorHandler<IApplicationModel> tashSelectorHandler,
            ITashCommunicator<IApplicationModel> tashCommunicator, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor,
            Dictionary<string, ISelector> selectors) : base(simpleLogger, tashSelectorHandler, tashCommunicator, selectors, methodNamesFromStackFramesExtractor) {
    }

    protected override Dictionary<string, ISimpleCollectionViewSourceHandler> CollectionViewSourceNamesToCollectionViewSourceHandlerDictionary(ITashTaskHandlingStatus<IApplicationModel> status) {
        throw new NotImplementedException();
    }

    protected override void OnValueTaskProcessed(ITashTaskHandlingStatus<IApplicationModel> status, bool verify, bool set, string actualValue) {
        if (!verify || actualValue == status.TaskBeingProcessed.Text) {
            status.Model.Status.Type = StatusType.Success;
            status.Model.Status.Text = "";
        }
        else {
            status.Model.Status.Type = StatusType.Error;
            status.Model.Status.Text = set
                ? $"Could not set {status.TaskBeingProcessed.ControlName} to \"{status.TaskBeingProcessed.Text}\", it is \"{actualValue}\""
                : $"Expected {status.TaskBeingProcessed.ControlName} to be \"{status.TaskBeingProcessed.Text}\", got \"{actualValue}\"";
        }
    }

    protected override Dictionary<string, ITextBox> TextBoxNamesToTextBoxDictionary(ITashTaskHandlingStatus<IApplicationModel> status) {
        return new() {
            { nameof(status.Model.ExpectedContents), status.Model.ExpectedContents },
            { nameof(status.Model.FormOrIdOrClassInstanceNumber), status.Model.FormOrIdOrClassInstanceNumber },
            { nameof(status.Model.FreeText), status.Model.FreeText },
            { nameof(status.Model.NewScriptName), status.Model.NewScriptName },
            { nameof(status.Model.Status), status.Model.Status },
            { nameof(status.Model.WebViewUrl), status.Model.WebViewUrl },
            { nameof(status.Model.WebViewCheckBoxesChecked), status.Model.WebViewCheckBoxesChecked },
            { nameof(status.Model.WebViewInputValues), status.Model.WebViewInputValues },
            { nameof(status.Model.WebViewParagraphs), status.Model.WebViewParagraphs },
            { nameof(status.Model.WebViewSelectedValues), status.Model.WebViewSelectedValues }
        };
    }

    protected override Dictionary<string, ISimpleTextHandler> TextBoxNamesToTextHandlerDictionary(ITashTaskHandlingStatus<IApplicationModel> status) {
        return new();
    }

    protected override Dictionary<string, ICollectionViewSource> CollectionViewSourceNamesToCollectionViewSourceDictionary(ITashTaskHandlingStatus<IApplicationModel> status) {
        return new();
    }
}