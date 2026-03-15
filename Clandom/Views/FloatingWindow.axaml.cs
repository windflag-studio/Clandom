using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Clandom.Views;

public partial class FloatingWindow : Window
{
    public FloatingWindow()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void RunButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var resultWindow = new ResultWindow();
        resultWindow.Show();
    }
}