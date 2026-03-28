using Avalonia;
using System;
using Avalonia.Media;

namespace Clandom;

sealed class Program
{
    /// <summary>
    /// Fluent Icons 字体
    /// </summary>
    public static FontFamily FluentIconsFontFamily { get; } =
        new FontFamily("avares://Clandom/Assets/Fonts/FluentSystemIcons-Resizable.ttf#FluentSystemIcons-Resizable");

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}