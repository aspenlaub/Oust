using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Outrap;

public class OutOfControl : IOutOfControl {
    public string Id { get; set; }
    public string Name { get; set; }
    public string Class { get; set; }
    public OutrapControlTypes Type { get; set; }
    public bool DynamicId;
    public List<IOutOfControl> ChildControls;

    public OutOfControl() {
        Class = "OutOfControl";
        Name = "NotAControl";
        Id = Guid.NewGuid().ToString();
        ChildControls = new List<IOutOfControl>();
        DynamicId = false;
        Type = OutrapControlTypes.None;
    }

    public int Depth() {
        if (!ChildControls.Any()) { return 1; }

        return 1 + ChildControls.Max(c => c.Depth());
    }

    public override string ToString() {
        if (Name == "") { return Name; }

        var s = Name + " (" + Enum.GetName(typeof(OutrapControlTypes), Type) + ")";
        return s;
    }

    public List<IOutOfControl> TraverseChildren() {
        return TraverseChildren(true);
    }

    public List<IOutOfControl> TraverseChildren(bool top) {
        var list = new List<IOutOfControl>();
        if (!top) { list.Add(this); }
        foreach(var childControl in ChildControls) {
            list.AddRange(childControl.TraverseChildren(false));
        }

        return list;
    }
}