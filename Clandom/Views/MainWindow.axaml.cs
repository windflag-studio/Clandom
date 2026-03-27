using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using FluentAvalonia.UI.Controls;

namespace Clandom.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // var page = Activator.CreateInstance(Type.GetType("Clandom.Views.Pages.RandomPage") ?? throw new InvalidOperationException());
        // NavigationView.Content = page;
        NavigationView.SelectedItem = HomePage;
    }

    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            var page = Activator.CreateInstance(Type.GetType("Clandom.Views.Pages.SettingsPage") ??
                                                throw new InvalidOperationException());
            (sender as NavigationView).Content = page;
        }
        else if (e.SelectedItem is NavigationViewItem item)
        {
            var prePage = $"Clandom.Views.Pages.{item.Tag}";
            var page = Activator.CreateInstance(Type.GetType(prePage) ?? throw new InvalidOperationException());
            (sender as NavigationView).Content = page;
        }
    }

    private async void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        var dialog = new ContentDialog()
        {
            Title = "确认退出？",
            Content = "如果点击“确定”，悬浮窗将一并关闭，而“最小化到托盘”则不会。",
            PrimaryButtonText = "取消",
            SecondaryButtonText = "最小化到托盘",
            CloseButtonText = "确定"
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return;
        }
        // if (result == ContentDialogResult.Secondary)
        // {
        //     Hide();
        // }
        else
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
        }
    }
}