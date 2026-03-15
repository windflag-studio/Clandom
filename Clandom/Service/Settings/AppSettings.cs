using CommunityToolkit.Mvvm.ComponentModel;

namespace Clandom.Service.Settings;

public partial class AppSettings : ObservableObject
{
    [ObservableProperty]
    private bool _isFloatingWindowAutoOpened = false;

    [ObservableProperty]
    private int _minId = 1;

    [ObservableProperty]
    private int _maxId = 50;
}