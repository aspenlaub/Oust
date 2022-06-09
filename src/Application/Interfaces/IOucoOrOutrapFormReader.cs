namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IOucoOrOutrapFormReader {
    IOucoOrOutrapForm Read(string fileName, bool isOutrap);
}