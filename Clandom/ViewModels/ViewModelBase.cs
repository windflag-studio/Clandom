using Clandom.Service.Settings;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Clandom.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    //public AppSettings Settings => App.Settings;
    private static AppSettings _defaultSettings;
    public static AppSettings DefaultSettings
    {
        get => _defaultSettings ?? App.Settings;
        set => _defaultSettings = value;
    }

    public AppSettings Settings { get; }

    protected ViewModelBase(AppSettings settings = null)
    {
        Settings = settings ?? DefaultSettings;
    }
}