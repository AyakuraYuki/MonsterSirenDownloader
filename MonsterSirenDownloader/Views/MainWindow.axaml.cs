using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace MonsterSirenDownloader.Views;

public partial class MainWindow : Window
{
    private bool _autoScroll = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ConsoleScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        // 检查用户是否手动滚动了ScrollViewer，如果用户滚动到了底部附近（容差5像素），则启用自动滚动；如果用户向上滚动，则禁用自动滚动。
        var isAtBottom = Math.Abs(scrollViewer.Offset.Y - scrollViewer.ScrollBarMaximum.Y) < 5;
        _autoScroll = isAtBottom;
    }

    private void ConsoleOutput_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(TextBox.Text) && _autoScroll)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var scrollViewer = this.FindControl<ScrollViewer>("ConsoleScrollViewer");
                scrollViewer?.ScrollToEnd();
            }, DispatcherPriority.Background);
        }
    }
}