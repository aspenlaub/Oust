using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IFileDialogTrickster {
    Task<string> EnterFileNameAndHaveOpenButtonPressedReturnErrorMessageAsync(string fileName, string windowName);
}