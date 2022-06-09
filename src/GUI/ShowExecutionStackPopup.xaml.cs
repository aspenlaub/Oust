using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.GUI;

/// <summary>
/// Interaction logic for ShowExecutionStackPopup.xaml
/// </summary>
public partial class ShowExecutionStackPopup : IShowExecutionStackPopup {
    public ShowExecutionStackPopup() {
        InitializeComponent();
    }

    public void ShowDialog(IList<string> formattedExecutionStack) {
        Dispatcher.Invoke(() => {
            FormattedStackTextBox.Text = string.Join("\r\n\r\n" + Properties.Resources.CalledFrom + "\r\n", formattedExecutionStack);
            ShowDialog();
        });
    }
}