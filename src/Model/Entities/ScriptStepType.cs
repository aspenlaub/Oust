namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;

public enum ScriptStepType {
    GoToUrl = 1, With = 2, Recognize = 3, Check = 4, Uncheck = 5, Press = 6, Input = 7, Select = 8, SubScript = 9, WithIdOrClass = 10,
    NotExpectedContents = 11, NotExpectedIdOrClass = 12, InputIntoSingle = 13, PressSingle = 14, EndOfScript = 15, WaitAMinute = 16,
    CheckSingle = 17, UncheckSingle = 18, WaitTenSeconds = 19, InvokeUrl = 20, RecognizeSelection = 21, NotExpectedSelection = 22,
    RecognizeOkay = 23
}