using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.GUI;

/// <summary>
/// Interaction logic for SelectScriptFromListPopup.xaml
/// </summary>
public partial class SelectScriptFromListPopup : ISelectScriptFromListPopup {
    private List<string> _SelectableScriptNames;
    private string _Filter;
    private string _Result;

    public SelectScriptFromListPopup() {
        InitializeComponent();
        _SelectableScriptNames = new List<string>();
        _Filter = "";
        _Result = "";
        SetSelectableScriptsAccordingToFilter();
    }

    public string ShowDialog(IList<string> selectableScriptNames) {
        _SelectableScriptNames = selectableScriptNames.ToList();
        _Filter = "";
        SetSelectableScriptsAccordingToFilter();
        Dispatcher.Invoke(() => {
            ShowDialog();
            return _Result;
        });

        return _Result;
    }

    private void ScriptNamesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        var items = e.AddedItems;
        if (items.Count != 1) { return; }

        _Result = (string)items[0];
        Close();
    }

    private void Filter_OnTextChanged(object sender, TextChangedEventArgs e) {
        _Filter = Filter.Text;
        SetSelectableScriptsAccordingToFilter();
    }

    private void SetSelectableScriptsAccordingToFilter() {
        ScriptNamesList.ItemsSource = _SelectableScriptNames.Where(x => x.Contains(_Filter, StringComparison.InvariantCultureIgnoreCase)).ToList();
    }
}