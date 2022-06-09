using System.Linq;
using System.Windows;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
#if DEBUG
        if (e.Args.Any(a => a == "/UnitTest")) {
            Context.DefaultEnvironmentType = EnvironmentType.UnitTest;
        }
#endif
    }
}