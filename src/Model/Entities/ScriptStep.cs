using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Model.Entities;

public class ScriptStep : IGuid, ISetGuid, IScriptStep {
    [Key, XmlAttribute("guid")]
    public string Guid { get; set; }

    [XmlAttribute("stepnumber")]
    public int StepNumber { get; set; }

    [XmlAttribute("steptype")]
    public ScriptStepType ScriptStepType { get; set; }

    [XmlAttribute("url")]
    public string Url { get; set; } = "";

    [XmlAttribute("form")]
    public string FormGuid { get; set; } = "";

    [XmlAttribute("formname")]
    public string FormName { get; set; } = "";

    [XmlAttribute("forminstance")]
    public int FormInstanceNumber { get; set; }

    [XmlAttribute("control")]
    public string ControlGuid { get; set; } = "";

    [XmlAttribute("controlname")]
    public string ControlName { get; set; } = "";

    [XmlAttribute("expectedcontents")]
    public string ExpectedContents { get; set; } = "";

    [XmlAttribute("inputtext")]
    public string InputText { get; set; } = "";

    [XmlAttribute("subscript")]
    public string SubScriptGuid { get; set; } = "";

    [XmlAttribute("subscriptname")]
    public string SubScriptName { get; set; } = "";

    [XmlAttribute("dividorclass")]
    public string IdOrClass { get; set; } = "";

    [XmlAttribute("divinstance")]
    public int IdOrClassInstanceNumber { get; set; }

    public ScriptStep() {
        Guid = System.Guid.NewGuid().ToString();
        ScriptStepType = ScriptStepType.GoToUrl;
        Url = "";
        FormGuid = "";
        FormName = "";
        FormInstanceNumber = 0;
        ControlGuid = "";
        ControlName = "";
        ExpectedContents = "";
        InputText = "";
        SubScriptGuid = "";
        SubScriptName = "";
        IdOrClass = "";
        IdOrClassInstanceNumber = 0;
    }

    public ScriptStep(ScriptStep scriptStep) {
        Guid = System.Guid.NewGuid().ToString();
        ReplaceWith(scriptStep);
    }

    public void ReplaceWith(ScriptStep scriptStep) {
        ScriptStepType = scriptStep.ScriptStepType;
        Url = scriptStep.Url;
        FormGuid = scriptStep.FormGuid;
        FormName = scriptStep.FormName;
        FormInstanceNumber = scriptStep.FormInstanceNumber;
        ControlGuid = scriptStep.ControlGuid;
        ControlName = scriptStep.ControlName;
        ExpectedContents = scriptStep.ExpectedContents;
        InputText = scriptStep.InputText;
        SubScriptGuid = scriptStep.SubScriptGuid;
        SubScriptName = scriptStep.SubScriptName;
        IdOrClass = scriptStep.IdOrClass;
        IdOrClassInstanceNumber = scriptStep.IdOrClassInstanceNumber;
    }

    public override string ToString() {
        switch (ScriptStepType) {
            case ScriptStepType.GoToUrl: {
                return "→ " + Url;
            }
            case ScriptStepType.InvokeUrl: {
                return "⬤ " + Url;
            }
            case ScriptStepType.With: {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (FormInstanceNumber == 1) {
                    return string.Format(Properties.Resources.WithFirst, FormName);
                }

                if (FormInstanceNumber == 2) {
                    return string.Format(Properties.Resources.WithSecond, FormName);
                }

                if (FormInstanceNumber == 3) {
                    return string.Format(Properties.Resources.WithThird, FormName);
                }

                return string.Format(Properties.Resources.WithNth, FormInstanceNumber, FormName);
            }
            case ScriptStepType.Recognize:
            case ScriptStepType.RecognizeSelection: {
                if (ExpectedContents == "") {
                    return string.Format(Properties.Resources.Recognize, ControlName);
                }

                if (string.IsNullOrWhiteSpace(ControlName)) {
                    return string.Format(Properties.Resources.RecognizeIdOrClassContents, ExpectedContents);
                }

                return string.Format(ScriptStepType == ScriptStepType.RecognizeSelection ? Properties.Resources.RecognizeSelected : Properties.Resources.RecognizeWithContents, ControlName, ExpectedContents);
            }
            case ScriptStepType.NotExpectedContents:
            case ScriptStepType.NotExpectedSelection: {
                if (string.IsNullOrWhiteSpace(ControlName)) {
                    return string.Format(Properties.Resources.NotExpectedContents, ExpectedContents);
                }

                return string.Format(ScriptStepType == ScriptStepType.NotExpectedSelection ? Properties.Resources.NotExpectedSelection : Properties.Resources.NotExpectedContentsInControl, ExpectedContents, ControlName);
            }
            case ScriptStepType.Check: {
                return string.Format(Properties.Resources.Check, ControlName);
            }
            case ScriptStepType.Uncheck: {
                return string.Format(Properties.Resources.Uncheck, ControlName);
            }
            case ScriptStepType.Press: {
                return string.Format(Properties.Resources.Press, ControlName);
            }
            case ScriptStepType.Input: {
                return string.Format(Properties.Resources.InputText, InputText, ControlName);
            }
            case ScriptStepType.Select: {
                return string.Format(Properties.Resources.SelectOption, InputText, ControlName);
            }
            case ScriptStepType.SubScript: {
                return string.Format(Properties.Resources.SubScript, SubScriptName);
            }
            case ScriptStepType.WithIdOrClass: {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (IdOrClassInstanceNumber == 1) {
                    return string.Format(Properties.Resources.WithFirstElement, IdOrClass);
                }

                if (IdOrClassInstanceNumber == 2) {
                    return string.Format(Properties.Resources.WithSecondElement, IdOrClass);
                }

                if (IdOrClassInstanceNumber == 3) {
                    return string.Format(Properties.Resources.WithThirdElement, IdOrClass);
                }

                return string.Format(Properties.Resources.WithNthElement, IdOrClassInstanceNumber, IdOrClass);
            }
            case ScriptStepType.NotExpectedIdOrClass: {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (IdOrClassInstanceNumber == 1) {
                    return string.Format(Properties.Resources.NotExpectedFirstIdOrClass, IdOrClass);
                }

                if (IdOrClassInstanceNumber == 2) {
                    return string.Format(Properties.Resources.NotExpectedSecondIdOrClass, IdOrClass);
                }

                if (IdOrClassInstanceNumber == 3) {
                    return string.Format(Properties.Resources.NotExpectedThirdIdOrClass, IdOrClass);
                }

                return string.Format(Properties.Resources.NotExpectedNthIdOrClass, IdOrClassInstanceNumber, IdOrClass);
            }
            case ScriptStepType.InputIntoSingle: {
                return string.Format(Properties.Resources.InputIntoSingleText, InputText);
            }
            case ScriptStepType.PressSingle: {
                return string.Format(Properties.Resources.PressSingle);
            }
            case ScriptStepType.EndOfScript: {
                return Properties.Resources.EndOfScript;
            }
            case ScriptStepType.WaitAMinute: {
                return string.Format(Properties.Resources.WaitAMinute);
            }
            case ScriptStepType.CheckSingle: {
                return string.Format(Properties.Resources.CheckSingle);
            }
            case ScriptStepType.UncheckSingle: {
                return string.Format(Properties.Resources.UncheckSingle);
            }
            case ScriptStepType.WaitTenSeconds: {
                return string.Format(Properties.Resources.WaitTenSeconds);
            }
            case ScriptStepType.RecognizeOkay: {
                return string.Format(Properties.Resources.RecognizeOkay);
            }
            default: {
                return Properties.Resources.StepTypeCannotBeDisplayed;
            }
        }
    }
}