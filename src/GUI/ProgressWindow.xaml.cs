using System.ComponentModel;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.GUI;

/// <summary>
/// Interaction logic for ProgressWindow.xaml
/// </summary>
public partial class ProgressWindow : IProgressWindow {
    private bool _ApplicationIsShuttingDown;

    public ProgressWindow() {
        InitializeComponent();
    }

    public void Show(string caption) {
        Dispatcher.Invoke(() => {
            Title = caption;
            MessagesTextBox.Text = "";
            Show();
        });
    }

    public void AddMessage(string message) {
        Dispatcher.Invoke(() => {
            MessagesTextBox.Text = MessagesTextBox.Text + (MessagesTextBox.Text.Length == 0 ? "" : "\r\n") + message;
            MessagesTextBox.ScrollToEnd();
        });
    }

    public void OnApplicationShutdown() {
        _ApplicationIsShuttingDown = true;
        Close();
    }

    private void ProgressWindow_OnClosing(object sender, CancelEventArgs e) {
        if (_ApplicationIsShuttingDown) { return; }

        Hide();
        e.Cancel = true;
    }
}