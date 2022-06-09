using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Ouco;

public class OucoOrOutrapForm : OutOfControl, IOucoOrOutrapForm {
    public override string ToString() {
        return Name;
    }
}