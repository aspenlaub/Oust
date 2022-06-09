using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IOucoHelper {
    Task<List<Selectable>> FormChoicesAsync();
    Task<List<Selectable>> OutOfControlChoicesAsync(ScriptStepType scriptStepType, string oucoFormGuid, int oucoFormInstanceNumber);
    Task<List<Selectable>> IdOrClassChoicesAsync();
    Task<List<Selectable>> SelectionChoicesAsync(string oucoFormGuid, int oucoFormInstanceNumber, string outOfControlGuid);
    Task<Dictionary<string, string>> AuxiliaryDictionaryAsync();
}