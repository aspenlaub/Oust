using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

public interface IOutrapHelper {
    Task<List<Selectable>> FormChoicesAsync();
    Task<List<Selectable>> OutOfControlChoicesAsync(ScriptStepType scriptStepType, string outrapFormGuid, int outrapFormInstanceNumber);
    Task<List<Selectable>> IdOrClassChoicesAsync();
    Task<List<Selectable>> SelectionChoicesAsync(string outrapFormGuid, int outrapFormInstanceNumber, string outOfControlGuid);
    Task<Dictionary<string, string>> AuxiliaryDictionaryAsync();
}