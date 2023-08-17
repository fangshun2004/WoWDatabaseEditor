using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaStyles.Controls.FastTableView;
using WDE.Common.Utils;
using WDE.DatabaseDefinitionEditor.ViewModels;
using WDE.DatabaseDefinitionEditor.ViewModels.DefinitionEditor;

namespace WDE.DatabaseDefinitionEditor.Views;

public partial class DefinitionEditorView : UserControl
{
    public DefinitionEditorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var TabControl = this.Find<TabControl>("TabControl");
        var RawJsonTab = this.Find<TabItem>("RawJsonTab");
        if (TabControl == null)
            return;

        if (DataContext is not DefinitionEditorViewModel vm)
            return;
            
        if (noUpdateJson)
            return;

        if (ignoreNextSelectionEvent > 0)
        {
            if (--ignoreNextSelectionEvent == 1)
                TabControl.SelectedItem = RawJsonTab;
            return;
        }
        
        if (e.AddedItems.Count == 1 && ReferenceEquals(e.AddedItems[0], RawJsonTab))
        {
            vm.UpdateRawDefinition();
        }
        else if (e.RemovedItems.Count == 1 && ReferenceEquals(e.RemovedItems[0], RawJsonTab))
        {
            UpdateViewModelFromJsonOrReturnToJsonTab().ListenErrors();
        }
    }

    private bool noUpdateJson;
    private int ignoreNextSelectionEvent; // some avalonia bug? that selection event is triggered when I press the control
    private async Task UpdateViewModelFromJsonOrReturnToJsonTab()
    {
        if (DataContext is not DefinitionEditorViewModel vm)
            return;
        
        if (!await vm.UpdateViewModelFromDefinition())
        {
            var TabControl = this.Get<TabControl>("TabControl");
            var RawJsonTab = this.Get<TabItem>("RawJsonTab");
            
            noUpdateJson = true;
            ignoreNextSelectionEvent = 2;
            TabControl.SelectedItem = RawJsonTab;
            noUpdateJson = false;
        }
    }
}