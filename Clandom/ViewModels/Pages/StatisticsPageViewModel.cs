using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace Clandom.ViewModels.Pages;

partial class StatisticsPageViewModel : ViewModelBase
{
    [ObservableProperty] private int[] _idCountsData;
    [ObservableProperty] private Axis[] _idLabelAxes = { new Axis { Labels = new List<string>() } };
    [ObservableProperty] private double[] _idWeightData;
    [ObservableProperty] private int[] _planeCountsData;

    [ObservableProperty] private Axis[] _planeLabelAxes = { new Axis { Labels = new List<string>() } };
    [ObservableProperty] private double[] _planeWeightData;
}