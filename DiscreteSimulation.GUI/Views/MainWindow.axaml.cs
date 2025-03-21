using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DiscreteSimulation.GUI.ViewModels;
using MsBox.Avalonia;
using ScottPlot;
using ScottPlot.AutoScalers;

namespace DiscreteSimulation.GUI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly List<Coordinates> _replicationsChartData = new();
    private readonly List<Coordinates> _dailyChartData = new();
    private readonly List<Coordinates> _cumulativeDailyChartData = new();
    private int _skipFirstNReplications = 0;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
        
        SetupCharts();
        
        _viewModel.Simulation.ReplicationEnded += ReplicationEnded;
        
        _viewModel.Simulation.NewDailyCosts += NewDailyCosts;

        _viewModel.Simulation.SimulationEnded += () =>
        {
            Dispatcher.UIThread.Post(() => _viewModel.EnableButtonsForSimulationEnd());
        };
        
        _viewModel.SelectedCustomFromFileStrategy += async () => await ShowLoadStrategyFromFileDialog();
    }

    private void ReplicationEnded()
    {
        if (_viewModel.Simulation.CurrentMaxReplications == 1)
        {
            var currentCostsForSingleReplication = _viewModel.Simulation.CurrentCosts;
            Dispatcher.UIThread.Post(() => NewReplicationResult(1, currentCostsForSingleReplication));
            return;
        }
        
        var currentReplication = _viewModel.Simulation.CurrentReplication;
        
        // V label popiskoch robime refresh bud kazdych 1000 replikacii
        // alebo podla nastavenia RenderOffset respektive poslednu replikaciu
        // (ak module nevyslo na 0) pre aktualnost
        if ((currentReplication % 1000 != 0) && (currentReplication - _skipFirstNReplications) % (_viewModel.RenderOffset + 1) != 0
            && currentReplication != _viewModel.Simulation.CurrentMaxReplications)
        {
            return;
        }
        
        var currentCosts = _viewModel.Simulation.CurrentCosts;
        
        Dispatcher.UIThread.Post(() => NewReplicationResult(currentReplication, currentCosts));
        
        // Berieme iba kazdu (RenderOffset + 1)-tu replikaciu pre vykreslenie
        // Ale ak sme uz na poslednej replikacii, a modulo nevyslo na 0,
        // tak sa zobrazi aj tak
        if ((currentReplication - _skipFirstNReplications) % (_viewModel.RenderOffset + 1) != 0 
            && currentReplication != _viewModel.Simulation.CurrentMaxReplications)
        {
            return;
        }
        
        // Preskocime prvych niekolko replikacii
        if (currentReplication < _skipFirstNReplications)
        {
            return;
        }
        
        Dispatcher.UIThread.Post(() => NewCostValue(currentReplication, currentCosts));
    }

    private async void StartSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.DisableButtonsForSimulationStart();

        if (_viewModel.Replications == 1)
        {
            _viewModel.IsSingleReplication = true;
        }
        else
        {
            _viewModel.IsSingleReplication = false;
        }
        
        _replicationsChartData.Clear();
        _dailyChartData.Clear();
        _cumulativeDailyChartData.Clear();
        
        CostsPlot.Plot.Axes.AutoScale();
        DailyCostsPlot.Plot.Axes.AutoScale();
        CumulativeDailyCostsPlot.Plot.Axes.AutoScale();
        CostsPlot.Refresh();
        
        _skipFirstNReplications = Convert.ToInt32(_viewModel.Replications * (_viewModel.SkipFirstNReplicationsInPercent / 100.0));
        
        _viewModel.Simulation.SelectedStrategy = _viewModel.SelectedStrategy;
        
        await Task.Run(() => _viewModel.Simulation.StartSimulation(_viewModel.Replications));
    }
    
    private void NewReplicationResult(long replication, double costs)
    {
        _viewModel.CurrentReplication = replication.ToString();
        _viewModel.CurrentCosts = costs.ToString("F2");
    }
    
    private void NewCostValue(long replication, double costs)
    {
        _replicationsChartData.Add(new Coordinates(replication, costs));

        CostsPlot.Plot.Axes.AutoScale();
        CostsPlot.Refresh();
    }

    private void StopSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.Simulation.StopSimulation();
    }
    
    private void NewDailyCosts(int day, double dailyCosts, double totalCosts)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _dailyChartData.Add(new Coordinates(day, dailyCosts));
            _cumulativeDailyChartData.Add(new Coordinates(day, totalCosts));
            
            DailyCostsPlot.Plot.Axes.AutoScale();
            DailyCostsPlot.Refresh();
            
            CumulativeDailyCostsPlot.Plot.Axes.AutoScale();
            CumulativeDailyCostsPlot.Refresh();
        });
    }
    
    private void SetupCharts()
    {
        var scatterLineReplications = CostsPlot.Plot.Add.ScatterLine(_replicationsChartData, Colors.Red);
        scatterLineReplications.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        CostsPlot.Plot.Axes.Bottom.Label.Text = "Replication";
        CostsPlot.Plot.Axes.Left.Label.Text = "Costs";
        
        DailyCostsPlot.Plot.Add.ScatterLine(_dailyChartData, Colors.Green);
        
        DailyCostsPlot.Plot.Axes.Bottom.Label.Text = "Day";
        DailyCostsPlot.Plot.Axes.Left.Label.Text = "Daily costs";
        
        var scatterLineCumulativeDaily = CumulativeDailyCostsPlot.Plot.Add.ScatterLine(_cumulativeDailyChartData, Colors.Blue);
        scatterLineCumulativeDaily.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        CumulativeDailyCostsPlot.Plot.Axes.Bottom.Label.Text = "Day";
        CumulativeDailyCostsPlot.Plot.Axes.Left.Label.Text = "Cumulative daily costs";
        
        CostsPlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);
        DailyCostsPlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);
        CumulativeDailyCostsPlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);
    }
    
    private async Task ShowLoadStrategyFromFileDialog()
    {
        var fileDialogResult = await StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "Vyberte súbor s vlastnou stratégiou",
            AllowMultiple = false,
            FileTypeFilter = [ FilePickerFileTypes.TextPlain ]
        });
        
        // Nebol vybrany subor s vlastnou strategiou - reset na strategiu A
        if (fileDialogResult.Count == 0)
        {
            _viewModel.SelectedStrategy = "A";
            return;
        }
        
        var file = fileDialogResult[0].Path;

        try
        {
            _viewModel.LoadCustomStrategyFromFile(file);
        }
        catch (Exception e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Failed to load custom strategy file.");
            await box.ShowWindowDialogAsync(this);
            
            _viewModel.SelectedStrategy = "A";
        }
    }
}
