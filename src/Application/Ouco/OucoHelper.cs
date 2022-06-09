using System.Text.Json;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Entities;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Entities;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Ouco;

public class OucoHelper : IOucoHelper {
    public IApplicationModel Model { get; init; }
    public IGuiAndWebViewAppHandler<ApplicationModel> GuiAndAppHandler { get; init; }
    public ISimpleLogger SimpleLogger { get; init; }
    private readonly OucoOrOutrapFormCollector _OucoOrOutrapFormCollector;
    private readonly IOustScriptStatementFactory _OustScriptStatementFactory;
    private readonly IMethodNamesFromStackFramesExtractor _MethodNamesFromStackFramesExtractor;

    protected static List<IOucoOrOutrapForm> Forms;

    public OucoHelper(IApplicationModel model, IGuiAndWebViewAppHandler<ApplicationModel> guiAndAppHandler, ISimpleLogger simpleLogger, IFolderResolver folderResolver, IOustScriptStatementFactory oustScriptStatementFactory, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor) {
        Model = model;
        GuiAndAppHandler = guiAndAppHandler;
        SimpleLogger = simpleLogger;
        _OucoOrOutrapFormCollector = new OucoOrOutrapFormCollector(new OucoOrOutrapFormReader(), folderResolver);
        _OustScriptStatementFactory = oustScriptStatementFactory;
        _MethodNamesFromStackFramesExtractor = methodNamesFromStackFramesExtractor;
    }

    public async Task<List<Selectable>> FormChoicesAsync() {
        var methodNamesFromStack = _MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
        SimpleLogger.LogInformationWithCallStack("Form choices requested", methodNamesFromStack);

        await SetFormsIfNecessaryAsync();

        var choices = new List<Selectable> {new() {Guid = "", Name = ""}};
        var formIds = new List<string>();

        var scriptStatement = new ScriptStatement { Statement = "OustUtilities.OucoOrOutrapFormIds()" };
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, false, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
            SimpleLogger.LogInformationWithCallStack("Form choices not available", methodNamesFromStack);
            return new List<Selectable>();
        }

        foreach (var form in Forms.Where(form => scriptCallResponse.Dictionary.Any(h => h.Key.StartsWith(form.Id, StringComparison.InvariantCulture))).Where(form => !formIds.Contains(form.Id))) {
            formIds.Add(form.Id);
            choices.Add(new Selectable { Guid = form.Id, Name = form.Name });
        }

        choices = choices.Count > 1 ? choices : new List<Selectable>();
        SimpleLogger.LogInformationWithCallStack($"Returning {choices.Count} form choice/-s", methodNamesFromStack);
        return choices;
    }

    public async Task<List<Selectable>> OutOfControlChoicesAsync(ScriptStepType scriptStepType, string oucoFormGuid, int oucoFormInstanceNumber) {
        var methodNamesFromStack = _MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
        SimpleLogger.LogInformationWithCallStack("Out-of-control choices requested", methodNamesFromStack);

        await SetFormsIfNecessaryAsync();

        var choices = new List<Selectable> {new() {Guid = "", Name = ""}};
        var form = Forms.FirstOrDefault(f => f.Id == oucoFormGuid);
        if (form == null) {
            SimpleLogger.LogInformationWithCallStack("Out-of-control choices not available (form not available", methodNamesFromStack);
            return new List<Selectable>();
        }

        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(oucoFormGuid, oucoFormInstanceNumber, oucoFormGuid);
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        if (!scriptCallResponse.Success.YesNo || scriptCallResponse.Success.Inconclusive) {
            SimpleLogger.LogInformationWithCallStack("Out-of-control choices not available (div not found", methodNamesFromStack);
            return new List<Selectable>();
        }

        var ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);

        foreach (var outOfControl in form.TraverseChildren().Where(outOfControl => DoesControlMatchScriptStepType(outOfControl, scriptStepType))) {
            scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, outOfControl.Id, "", "", 0);
            scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
            if (scriptCallResponse.Success.Inconclusive|| !scriptCallResponse.Success.YesNo) {
                continue;
            }

            choices.Add(new Selectable { Guid = outOfControl.Id, Name = outOfControl.ToString() });
        }

        choices = choices.Count > 1 ? choices : new List<Selectable>();
        SimpleLogger.LogInformationWithCallStack($"Returning {choices.Count} out-of-control choice/-s", methodNamesFromStack);
        return choices;
    }

    private bool DoesControlMatchScriptStepType(IOutOfControl outOfControl, ScriptStepType scriptStepType) {
        switch (scriptStepType) {
            case ScriptStepType.Check or ScriptStepType.Uncheck when outOfControl.Type != OucoControlTypes.CheckBox:
            case ScriptStepType.Press when outOfControl.Type != OucoControlTypes.Button && outOfControl.Type != OucoControlTypes.Upload:
            case ScriptStepType.Input when outOfControl.Type != OucoControlTypes.Input && outOfControl.Type != OucoControlTypes.Restricted && outOfControl.Type != OucoControlTypes.TextArea && outOfControl.Type != OucoControlTypes.Upload:
            case ScriptStepType.Select when outOfControl.Type != OucoControlTypes.DropDown:
                return false;
            default:
                return true;
        }
    }

    public async Task<List<Selectable>> IdOrClassChoicesAsync() {
        var methodNamesFromStack = _MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
        SimpleLogger.LogInformationWithCallStack("Id-or-class choices requested", methodNamesFromStack);

        var choices = new List<Selectable> { new() {Guid = "", Name = ""} };
        var scriptStatement = new ScriptStatement {
            Statement = "OustUtilities.IdsAndClasses()"
        };
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) { return new List<Selectable>(); }

        choices.AddRange(scriptCallResponse.Dictionary
            .Where(idAndClass => idAndClass.Key.Count(c => c == '-') <= 3)
            .Select(idAndClass => new Selectable { Guid = idAndClass.Key, Name = idAndClass.Value }));
        choices = choices.Count > 1 ? choices : new List<Selectable>();
        SimpleLogger.LogInformationWithCallStack($"Returning {choices.Count} id-or-class choice/-s", methodNamesFromStack);
        return choices;
    }

    public async Task<List<Selectable>> SelectionChoicesAsync(string oucoFormGuid, int oucoFormInstanceNumber, string outOfControlGuid) {
        var methodNamesFromStack = _MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
        SimpleLogger.LogInformationWithCallStack("Selection choices requested", methodNamesFromStack);

        await SetFormsIfNecessaryAsync();

        var choices = new List<Selectable>() { new() { Guid = "", Name = "" } };
        var form = Forms.FirstOrDefault(f => f.Id == oucoFormGuid);
        if (form == null) { return choices; }

        var scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentHaveDivLikeWithIdOrNthOccurrenceOfClassStatement(oucoFormGuid, oucoFormInstanceNumber, oucoFormGuid);
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
            SimpleLogger.LogInformationWithCallStack("Selection choices not available (div not found)", methodNamesFromStack);
            return choices;
        }

        var ancestorDomElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = _OustScriptStatementFactory.CreateDoesDocumentContainDescendantElementOfClassStatement(ancestorDomElementJson, outOfControlGuid, "", "", 0);
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
            SimpleLogger.LogInformationWithCallStack("Selection choices not available (select not found)", methodNamesFromStack);
            return choices;
        }

        var selectElementJson = JsonSerializer.Serialize(scriptCallResponse.DomElement);
        scriptStatement = new ScriptStatement {
            Statement = "OustUtilities.SelectOptions(" + selectElementJson + ")"
        };
        scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo) {
            SimpleLogger.LogInformationWithCallStack("Selection choices not available (options not available)", methodNamesFromStack);
            return choices;
        }

        choices.AddRange(scriptCallResponse.Dictionary.Select(option => new Selectable { Guid = option.Key, Name = option.Value }));

        choices = choices.Count > 1 ? choices : new List<Selectable>();
        SimpleLogger.LogInformationWithCallStack($"Returning {choices.Count} selection choice/-s", methodNamesFromStack);
        return choices;
    }

    public async Task SetFormsIfNecessaryAsync() {
        if (Forms != null) { return; }

        Forms = await _OucoOrOutrapFormCollector.GetFormsAsync();
    }

    public async Task<Dictionary<string, string>> AuxiliaryDictionaryAsync() {
        var scriptStatement = new ScriptStatement {
            Statement = "OustUtilities.AuxiliaryDictionary()"
        };
        var scriptCallResponse = await GuiAndAppHandler.RunScriptAsync<ScriptCallResponse>(scriptStatement, true, true);
        if (scriptCallResponse.Success.Inconclusive || !scriptCallResponse.Success.YesNo || scriptCallResponse.Dictionary.Count == 0) {
            return new Dictionary<string, string> {
                { "WebViewCheckBoxesChecked", "" }, { "WebViewParagraphs", "" }, { "WebViewInputValues", "" }, { "WebViewSelectedValues", "" }
            };
        }

        return scriptCallResponse.Dictionary;
    }
}