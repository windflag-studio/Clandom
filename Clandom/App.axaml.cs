using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Clandom.Service.Settings;
using Clandom.ViewModels;
using Clandom.Views;

namespace Clandom;

public class App : Application
{
    private TrayIcon _trayIcon;
    public static AppSettings Settings { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        base.Initialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Settings = SettingsManager.Load();
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.ShutdownRequested += OnShutdownRequested;
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            var showItem = new NativeMenuItem("显示窗口");
            showItem.Click += ShowWindow_Click;
            var exitItem = new NativeMenuItem("退出");
            exitItem.Click += Exit_Click;
            var iconUri = new Uri("avares://Clandom/Assets/avalonia-logo.ico");
            using var iconStream = AssetLoader.Open(iconUri);

            _trayIcon = new TrayIcon
            {
                Icon = new WindowIcon(iconStream),
                ToolTipText = "Clandom",
                Menu = new NativeMenu { Items = { showItem, exitItem } }
            };
            _trayIcon.IsVisible = true;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private void ShowWindow_Click(object? sender, EventArgs e)
    {
        var mainWindow = Application.Current?.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime;
        if (mainWindow?.MainWindow != null)
        {
            mainWindow.MainWindow.Show();
            mainWindow.MainWindow.WindowState = WindowState.Normal;
            mainWindow.MainWindow.Activate();
        }
    }

    private void Exit_Click(object? sender, EventArgs e)
    {
        _trayIcon?.Dispose(); // 移除托盘图标
        SettingsManager.Save(Settings);
        Environment.Exit(0);
    }

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        SettingsManager.Save(Settings);
    }
}