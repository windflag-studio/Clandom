using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Clandom.Service.Settings;

public partial class AppSettings : ObservableObject
{
    [ObservableProperty] private int _mainWindowWidth = 660;
    [ObservableProperty] private int _mainWindowHeight = 580;
    
    [ObservableProperty] private bool _isIdMode = true;
    [ObservableProperty] private int _minId = 1;
    [ObservableProperty] private int _maxId = 50;
    [ObservableProperty] private int _col = 8;
    [ObservableProperty] private int _row = 6;
}