using System.Net;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class StopCodeCoverageCommand : ICommand {
    private readonly IApplicationModel _Model;
    private readonly IDumperNameConverter _DumperNameConverter;
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndWebViewAppHandler;
    private readonly ILogicalUrlRepository _LogicalUrlRepository;

    public StopCodeCoverageCommand(IApplicationModel model, IDumperNameConverter dumperNameConverter,
                IGuiAndWebViewAppHandler<ApplicationModel> guiAndWebViewAppHandler, ILogicalUrlRepository logicalUrlRepository) {
        _Model = model;
        _DumperNameConverter = dumperNameConverter;
        _GuiAndWebViewAppHandler = guiAndWebViewAppHandler;
        _LogicalUrlRepository = logicalUrlRepository;
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await _LogicalUrlRepository.GetUrlAsync("SaveCodeCoverage", errorsAndInfos)
            + "?name="
            + WebUtility.UrlEncode(_DumperNameConverter.ScriptFileFriendlyShortName(_Model.SelectedScript.SelectedItem.Name));
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        await _GuiAndWebViewAppHandler.NavigateToUrlAsync(url);
    }
}