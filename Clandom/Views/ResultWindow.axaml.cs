using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Clandom.Models.BalancedRandom;
using Clandom.Service.Settings;
using SkiaSharp.HarfBuzz;

namespace Clandom.Views;

public partial class ResultWindow : Window
{
    public ResultWindow()
    {
        InitializeComponent();
    }

    private void WindowBase_OnDeactivated(object? sender, EventArgs e)
    {
        Close();
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        var settings = SettingsManager.Load();
        if (settings.MinId >= settings.MaxId)
        {
            return;
        }
        if (settings.IsIdMode)
        {
            var randId = new BalancedRand((int)settings.MinId, (int)settings.MaxId);
            Result.Text = randId.Draw().ToString();
        }
        else
        {
            var randIdPlane = new BalancedRandPlane((int)settings.Row, (int)settings.Col);
            var pos = randIdPlane.DrawPosition();
            Result.Text = $"行:{pos.row} 列:{pos.col}";
        }
    }
}