using System;
using System.Linq;
using System.Windows;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.GUI;

/// <summary>
/// Interaction logic for ExtractSubScriptPopup.xaml
/// </summary>
public partial class ExtractSubScriptPopup : IExtractSubScriptPopup {
    private readonly EnvironmentType _EnvironmentType;
    private readonly INewScriptNameValidator _NewScriptNameValidator;

    public ExtractSubScriptPopup(EnvironmentType environmentType, INewScriptNameValidator newScriptNameValidator) {
        InitializeComponent();
        _NewScriptNameValidator = newScriptNameValidator;
        _EnvironmentType = environmentType;
    }

    public IExtractSubScriptSpecification Show(ISelector selector) {
        IExtractSubScriptSpecification result = null;

        Dispatcher.Invoke(() => {
            NewSubScriptName.Text = "";
            ScriptStepsToExtract.Items.Clear();
            var index = 0;
            foreach (var selectable in selector.Selectables) {
                if (selectable.Name == Enum.GetName(typeof(ScriptStepType), ScriptStepType.EndOfScript)) { break; }

                ScriptStepsToExtract.Items.Add(new OrderedSelectable(selectable, index ++));
            }

            ShowDialog();

            result = new ExtractSubScriptSpecification(NewSubScriptName.Text, ScriptStepsToExtract.SelectedItems.Cast<Selectable>());
        });

        return result;
    }

    public void OnApplicationShutdown() {
        Close();
    }

    private async void ExtractSubScript_OnClickAsync(object sender, RoutedEventArgs e) {
        if (!await _NewScriptNameValidator.IsNewScriptNameValidAsync(_EnvironmentType, NewSubScriptName.Text)) {
            MessageBox.Show(Properties.Resources.SubScriptNameCannotBeUsed);
            return;
        }

        var orderedSelectables = ScriptStepsToExtract.SelectedItems.Cast<OrderedSelectable>().OrderBy(o => o.Sort).ToList();

        if (!orderedSelectables.Any()) {
            MessageBox.Show(Properties.Resources.NoScriptStepsHaveBeenSelected);
            return;
        }

        for (var i = 0; i < orderedSelectables.Count - 1; i++) {
            if (orderedSelectables[i].Sort + 1 == orderedSelectables[i + 1].Sort) {
                continue;
            }

            MessageBox.Show(Properties.Resources.OnlyConsecutiveStepsMayBeSelected);
            return;
        }

        Hide();
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e) {
        ScriptStepsToExtract.SelectedItems.Clear();
        Hide();
    }
}