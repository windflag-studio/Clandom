using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Clandom.Views;

public partial class FloatingWindow : Window
{
    private PixelPoint _dragStartOffset; // 鼠标按下时，鼠标相对于窗口左上角的偏移量（物理像素）
    private bool _isDragging;

    public FloatingWindow()
    {
        InitializeComponent();
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        // 获取鼠标相对于窗口内容区域的 DIP 坐标
        var relativePoint = e.GetPosition(this);
        // 转换为屏幕物理像素坐标
        var screenPoint = topLevel.PointToScreen(relativePoint);

        // 计算鼠标相对于窗口左上角的偏移量（物理像素）
        _dragStartOffset = new PixelPoint(
            screenPoint.X - Position.X,
            screenPoint.Y - Position.Y
        );

        _isDragging = true;
        // 捕获指针，确保移动事件能继续触发
        e.Pointer.Capture(sender as IInputElement);
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (!_isDragging) return;

        var topLevel = TopLevel.GetTopLevel(this);
        // 获取当前鼠标相对于窗口的 DIP 坐标
        var currentRelativePoint = e.GetPosition(this);
        // 转换为屏幕物理像素坐标
        var currentScreenPoint = topLevel.PointToScreen(currentRelativePoint);

        // 新窗口位置 = 当前鼠标屏幕坐标 - 偏移量
        var newPosition = new PixelPoint(
            currentScreenPoint.X - _dragStartOffset.X,
            currentScreenPoint.Y - _dragStartOffset.Y
        );

        Position = newPosition;
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
        }
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