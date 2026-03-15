using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Clandom.Control;

public partial class AutoSizingTextView : UserControl
{
    public AutoSizingTextView()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<AutoSizingTextView, string>(nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}