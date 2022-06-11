using Aspenlaub.Net.GitHub.CSharp.Oust.Application;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.GUI;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.GUI;

public class GuiAndApplicationSynchronizer : GuiAndWebViewApplicationSynchronizerBase<ApplicationModel, OustWindow>, IGuiAndApplicationSynchronizer {

    public GuiAndApplicationSynchronizer(ApplicationModel model, OustWindow window, ISimpleLogger simpleLogger,
                IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor, IOucidLogAccessor oucidLogAccessor)
                    : base(model, window, simpleLogger, methodNamesFromStackFramesExtractor, oucidLogAccessor) {
    }

    public override void OnCursorChanged() {
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(OnCursorChanged)))) {
            var methodNamesFromStack = MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
            SimpleLogger.LogInformationWithCallStack(Model.IsBusy ? Properties.Resources.IndicatingBusy : Properties.Resources.IndicatingIdle, methodNamesFromStack);
        }
    }
}