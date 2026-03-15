using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Clandom.Models.BalancedRandom;
using Clandom.ViewModels.Pages;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Clandom.Views.Pages;

public partial class StatisticsPage : UserControl
{
    private List<List<int>> _idData;
    private List<List<int>> _planeData;
    public StatisticsPage()
    {
        InitializeComponent();
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _idData = BalancedRandDataManager.GetAllIdData();
        _planeData = BalancedRandDataManager.GetAllPlaneData();
        foreach (var dataId in _idData)
        {
            IdStatisticsComboBox.Items.Add($"从{dataId[0]}到{dataId[1]}");
        }
        IdStatisticsComboBox.SelectedIndex = 0;
        
        foreach (var dataTd in _planeData)
        {
            PlaneStatisticsComboBox.Items.Add($"{dataTd[0]}行{dataTd[1]}列");
        }
        PlaneStatisticsComboBox.SelectedIndex = 0;
    }

    private void IdStatisticsComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsInitialized&&_idData.Count != 0)
        {
            StatisticsPageViewModel.IdCountsData = BalancedRandDataManager.GetDrawCountsByIdRange(_idData[IdStatisticsComboBox.SelectedIndex]).ToArray();
            StatisticsPageViewModel.IdWeightData = BalancedRandDataManager.GetWeightsByIdRange(_idData[IdStatisticsComboBox.SelectedIndex]).ToArray();
            (DataContext as StatisticsPageViewModel).RefreshIdSeries();
        }
    }

    private void TDStatisticsComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsInitialized && _planeData != null && _planeData.Count > 0 && 
            PlaneStatisticsComboBox.SelectedIndex >= 0)
        {
            try
            {
                var range = _planeData[PlaneStatisticsComboBox.SelectedIndex];
                int rows = range[0], cols = range[1];

                var countsDict = BalancedRandDataManager.GetDrawCountsByPlaneRange(range);
                var weightsDict = BalancedRandDataManager.GetWeightsByPlaneRange(range);

                var orderedCounts = new List<int>();
                var orderedWeights = new List<double>();
                var orderedLabels = new List<string>();

                for (int r = 1; r <= rows; r++)
                {
                    for (int c = 1; c <= cols; c++)
                    {
                        countsDict.TryGetValue((r, c), out int count);
                        orderedCounts.Add(count);

                        weightsDict.TryGetValue((r, c), out double weight);
                        orderedWeights.Add(weight);

                        orderedLabels.Add($"[{r},{c}]");
                    }
                }

                StatisticsPageViewModel.PlaneCountsData = orderedCounts.ToArray();
                StatisticsPageViewModel.PlaneWeightData = orderedWeights.ToArray();
                (DataContext as StatisticsPageViewModel).PlaneLabelData = orderedLabels.ToArray();
                (DataContext as StatisticsPageViewModel).RefreshPlaneSeries();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载平面统计数据失败: {ex.Message}");
            }
        }
    }
}