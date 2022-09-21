namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IOutrapFormReader {
    IOutrapForm Read(string fileName, bool isOutrap);
}