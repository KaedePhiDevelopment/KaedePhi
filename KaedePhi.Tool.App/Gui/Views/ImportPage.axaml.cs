using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using KaedePhi.Tool.App.Gui.ViewModels;
using KaedePhi.Tool.Converter;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.App.Gui.Views;

public partial class ImportPage : UserControl
{
    private bool _isPicking;
    private Border? _dropOverlay;

    public ImportPage()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _dropOverlay = this.FindControl<Border>("DropOverlay");
    }

    private void ShowOverlay()
    {
        if (_dropOverlay != null)
            _dropOverlay.IsVisible = true;
    }

    private void HideOverlay()
    {
        if (_dropOverlay != null)
            _dropOverlay.IsVisible = false;
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (DataContext is ImportViewModel { IsLoading: true })
            return;
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
            ShowOverlay();
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Contains(DataFormat.File)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        HideOverlay();
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        HideOverlay();

        if (DataContext is not ImportViewModel vm || vm.IsLoading)
            return;

        var path = e
            .DataTransfer.Items.Select(item =>
                item.TryGetRaw(DataFormat.File) is IStorageItem storageItem
                    ? storageItem.TryGetLocalPath()
                    : null
            )
            .FirstOrDefault(p => !string.IsNullOrEmpty(p));

        if (!string.IsNullOrEmpty(path))
            vm.OnFileSelected(path);
    }

    private void OnCancelLoadingClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ImportViewModel vm)
            vm.OnCancelLoadingClicked();
    }

    private async void OnImportClick(object? sender, RoutedEventArgs e)
    {
        if (_isPicking)
            return;
        _isPicking = true;

        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
                return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = import_file_picker_title,
                    AllowMultiple = false,
                    FileTypeFilter = ChartFormatRegistry.ImportableFormats
                        .GroupBy(format => format.FileExtension, StringComparer.OrdinalIgnoreCase)
                        .Select(group => new FilePickerFileType(
                            group.Key.ToLowerInvariant() switch
                            {
                                "json" => file_type_json,
                                "pec" => file_type_pe_chart,
                                _ => string.Join(" / ", group.Select(format => format.FormatId)),
                            })
                        {
                            Patterns = [$"*.{group.Key}"],
                        })
                        .Append(new FilePickerFileType(file_type_all) { Patterns = ["*.*"] })
                        .ToArray(),
                }
            );

            if (files.Count > 0 && DataContext is ImportViewModel vm)
            {
                var path = files[0].TryGetLocalPath();
                if (!string.IsNullOrEmpty(path))
                {
                    vm.OnFileSelected(path);
                }
            }
        }
        finally
        {
            _isPicking = false;
        }
    }
}
