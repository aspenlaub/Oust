using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Commands;

public class ShowExecutionStackCommand : ICommand {
    private readonly IApplicationModel _Model;
    private readonly IShowExecutionStackPopupFactory _PopupFactory;
    private readonly IExecutionStackFormatter _Formatter;

    public ShowExecutionStackCommand(IApplicationModel model, IShowExecutionStackPopupFactory popupFactory, IExecutionStackFormatter formatter) {
        _Model = model;
        _PopupFactory = popupFactory;
        _Formatter = formatter;
    }

    public async Task<bool> ShouldBeEnabledAsync() {
        return await Task.FromResult(_Model.ExecutionStackItems.ToList().Any());
    }

    public async Task ExecuteAsync() {
        _Model.IsBusy = false;

        var formattedStack = await _Formatter.FormatExecutionStackAsync(_Model.EnvironmentType, _Model.ScriptSteps.SelectedItem.Guid, _Model.ExecutionStackItems.ToList());
        _PopupFactory.Create().ShowDialog(formattedStack);
        await Task.Delay(TimeSpan.FromMilliseconds(10));
    }
}