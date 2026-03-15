using Clandom.Service.Settings;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Clandom.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public AppSettings Settings => App.Settings;
}