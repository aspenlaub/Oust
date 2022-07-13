using System.Xml;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application.Ouco;

public class OucoOrOutrapFormReader : IOucoOrOutrapFormReader {
    public IOucoOrOutrapForm Read(string fileName, bool isOutrap) {
        var doc = new XmlDocument();
        doc.Load(fileName);
        var root = doc.DocumentElement;
        var classAttributeValue = root?.Attributes["class"]?.Value;
        if (classAttributeValue == null) { return null; }

        var form = new OucoOrOutrapForm {
            Class = classAttributeValue,
            Name = root.HasAttribute("name") && root.Attributes["name"] != null ? root.Attributes["name"].Value : classAttributeValue,
            Type = OucoControlTypes.OucoFormDefinition
        };
        XmlNode formDoc = null, guidsOrUidsDoc = null;
        var found = false;
        for (var i = 0; i < doc.ChildNodes.Count; i++) {
            var subDoc = doc.ChildNodes[i];
            if (subDoc == null) { continue; }

            switch (subDoc.Name) {
                case "form":
                    if (subDoc.ChildNodes.Count != 2) { return form; }

                    found = true;
                    formDoc = subDoc;
                    guidsOrUidsDoc = subDoc.ChildNodes[1];
                    break;
                case "outrappage":
                case "outrapform":
                    if (subDoc.ChildNodes.Count < 2) { return form; }

                    found = true;
                    formDoc = subDoc;
                    guidsOrUidsDoc = subDoc.ChildNodes.Cast<XmlNode>().Single(c => c.Name == "uids");
                    form.Type = OucoControlTypes.OutrapForm;
                    break;
            }

            if (found) {
                break;
            }
        }

        if (guidsOrUidsDoc == null) { return form; }

        var guidsOrUids = ReadGuidsOrUids(guidsOrUidsDoc, form.Type);
        if (!guidsOrUids.ContainsKey(form.Name)) { return form; }

        form.Id = guidsOrUids[form.Name];
        ReadChildNodes(form, formDoc, guidsOrUids, isOutrap);
        return form;
    }

    protected Dictionary<string, string> ReadGuidsOrUids(XmlNode doc, OucoControlTypes controlType) {
        var guidsOrUids = new Dictionary<string, string>();
        for (var i = 0; i < doc.ChildNodes.Count; i++) {
            var subDoc = doc.ChildNodes[i];
            if (subDoc?.Attributes?["name"] == null || subDoc.Attributes["id"] == null) { continue; }

            guidsOrUids[subDoc.Attributes["name"].Value] = subDoc.Attributes["id"].Value;
        }

        return guidsOrUids;
    }

    protected void ReadChildNodes(OutOfControl parentControl, XmlNode doc, Dictionary<string, string> guids, bool isOutrap) {
        for (var i = 0; i < doc.ChildNodes.Count; i++) {
            var subDoc = doc.ChildNodes[i];
            if (subDoc == null || subDoc.Name == "guids" || subDoc.Name == "pseudo" || subDoc.Name == "uids" || subDoc.Name == "onetoones") { continue; }

            var attributes = AttributeDictionary(subDoc);
            string name;
            if (attributes.ContainsKey("name")) {
                name = attributes["name"];
            } else if (subDoc.Name == "upload") {
                name = "UploadControl";
            } else {
                continue;
            }

            if (!guids.ContainsKey(name)) {
                continue;
            }

            var guid = guids[name];
            var oclass = attributes.ContainsKey("class") ? attributes["class"] : "";
            if (guid.Length == 0) {
                throw new NotImplementedException();
            }

            var childControl = new OutOfControl { Name = name, Id = guid };
            switch (subDoc.Name) {
                case "button": childControl.Type = OucoControlTypes.Button; break;
                case "checkbox": childControl.Type = OucoControlTypes.CheckBox; break;
                case "copy": childControl.Type = OucoControlTypes.Copy; break;
                case "dropdown": childControl.Type = OucoControlTypes.DropDown; break;
                case "vstack":
                case "hstack": childControl.Type = OucoControlTypes.Stack; break;
                case "input": childControl.Type = OucoControlTypes.Input; break;
                case "legacymenu": childControl.Type = OucoControlTypes.LegacyMenu; childControl.Class = oclass; break;
                case "menu": childControl.Type = OucoControlTypes.Menu; childControl.Class = oclass; break;
                case "subform": childControl.Type = OucoControlTypes.SubForm; childControl.Class = oclass; break;
                case "restricted": childControl.Type = OucoControlTypes.Restricted; break;
                case "table": childControl.Type = OucoControlTypes.Table; break;
                case "textarea": childControl.Type = OucoControlTypes.TextArea; break;
                case "valoutput":
                    childControl.Type = OucoControlTypes.ValidationOutput;
                    if (isOutrap) {
                        childControl.Id = childControl.Id + "-ValOut";
                    }
                    break;
                case "upload": childControl.Type = OucoControlTypes.Upload; break;
                case "frame": childControl.Type = OucoControlTypes.Frame; break;
                case "img": childControl.Type = OucoControlTypes.Image; break;
                case "row": childControl.Type = OucoControlTypes.Row; break;
                case "col": childControl.Type = OucoControlTypes.Column; break;
                case "navtabs": childControl.Type = OucoControlTypes.NavTabs; break;
                case "card": childControl.Type = OucoControlTypes.Card; break;
                case "carousel": childControl.Type = OucoControlTypes.Carousel; break;
                default: {
                    throw new NotImplementedException($"{nameof(OucoOrOutrapFormReader)} does not know how to handle {subDoc.Name}");
                }
            }

            parentControl.ChildControls.Add(childControl);
            ReadChildNodes(childControl, subDoc, guids, isOutrap);
        }
    }

    protected Dictionary<string, string> AttributeDictionary(XmlNode doc) {
        var attributes = new Dictionary<string, string>();
        if (doc.Attributes == null) { return attributes; }

        for (var i = 0; i < doc.Attributes.Count; i++) {
            attributes[doc.Attributes[i].Name] = doc.Attributes[i].Value;
        }

        return attributes;
    }
}