using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Extensions;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class CodeCoverageCommand : ICommand {
    private readonly IGuiAndWebViewAppHandler<ApplicationModel> _GuiAndWebViewAppHandler;
    private readonly ILogicalUrlRepository _LogicalUrlRepository;

    public CodeCoverageCommand(IGuiAndWebViewAppHandler<ApplicationModel> guiAndWebViewAppHandler, ILogicalUrlRepository logicalUrlRepository) {
        _GuiAndWebViewAppHandler = guiAndWebViewAppHandler;
        _LogicalUrlRepository = logicalUrlRepository;
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(true);
    }

    public async Task ExecuteAsync() {
        var errorsAndInfos = new ErrorsAndInfos();
        var url = await _LogicalUrlRepository.GetUrlAsync("ActivateCodeCoverage", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        await _GuiAndWebViewAppHandler.NavigateToUrlAsync(url);
    }
}