using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Outrap;

public class OutrapForm : OutOfControl, IOutrapForm {
    public override string ToString() {
        return Name;
    }
}