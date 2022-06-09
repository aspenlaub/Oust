namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IOutOfControl {
    string Id { get; }
    string Name { get; }
    string Class { get; }
    OucoControlTypes Type { get; }
    int Depth();
    string ToString();
    List<IOutOfControl> TraverseChildren();
    List<IOutOfControl> TraverseChildren(bool top);
}