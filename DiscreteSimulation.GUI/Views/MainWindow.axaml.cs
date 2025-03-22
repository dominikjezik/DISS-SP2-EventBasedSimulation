using System;
using System.Collections.Generic;
using System.Linq;
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
        
        // SetupCharts();

        _viewModel.TicketStoreSimulation.SimulationStateChanged += SimulationStateChanged;
        
        // _viewModel.TicketStoreSimulation.ReplicationEnded += ReplicationEnded;

        _viewModel.TicketStoreSimulation.SimulationEnded += () =>
        {
            Dispatcher.UIThread.Post(() => _viewModel.EnableButtonsForSimulationEnd());
        };
    }

    private double _latestGUIUpdate = -1;
    private int _skippedGUIUpdates = 0;

    private void SimulationStateChanged()
    {
        var simulationTime = _viewModel.TicketStoreSimulation.SimulationTime;
        var simulationSpeed = _viewModel.TicketStoreSimulation.SimulationSpeed;

        if (!double.IsInfinity(simulationSpeed))
        {
            // Rozdiel posledneho updatu GUI a aktualneho casu simulacie
            var differenceCurrentAndLast = simulationTime - _latestGUIUpdate;
        
            // Parametre ovplyvnujuce rychlost updatovania GUI
            var sleepTime = 100; // TODO: vytiahnut na GUI
            var delta = simulationSpeed * sleepTime / 1000;
        
            // Rychlost updatovania GUI je zavisle od delta
            var customParameter = 0.025;
            var minimalDifference = delta * customParameter;
        
            if (differenceCurrentAndLast < minimalDifference)
            {
                return;
            }
            
            // delta 1, difference 1 -> ok
            // delta 1, difference 0.5 -> ok
            // delta 1, difference 0.2 -> ok
            // delta 1, difference 0.1 -> skip
        
            // delta 2, difference 2 -> ok
            // delta 2, difference 1 -> ok
            // delta 1, difference 0.1 -> ok
            // delta 2, difference 0.2 -> skip
        }
        else
        {
            // Ak je rychlost nekonecna, updatovanie nemoze byt zavisle od simulačného času pretože, updaty
            // neprichádzajú cca v ekvidistantných časových intervaloch, preto pocitame preskocenie updatov 
            if (_skippedGUIUpdates <= 10_000)
            {
                _skippedGUIUpdates++;
                return;
            }
            
            _skippedGUIUpdates = 0;
        }
        
        
        _latestGUIUpdate = simulationTime;
        
        var averageWaitingTime = _viewModel.TicketStoreSimulation.AverageWaitingTime.Mean;
        var averageQueueLength = _viewModel.TicketStoreSimulation.CustomersQueue.AverageQueueLength;
        
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.CurrentSimulationTime = simulationTime.ToString("F2");
            _viewModel.AvgOrderProcessingTime = averageWaitingTime.ToString("F2");
            _viewModel.AvgPendingOrders = averageQueueLength.ToString("F2");
        });
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
        
        Dispatcher.UIThread.Post(() => AddValueToChart(currentReplication, currentCosts));
    }

    private async void StartSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.DisableButtonsForSimulationStart();
        
        // _replicationsChartData.Clear();
        // _dailyChartData.Clear();
        // _cumulativeDailyChartData.Clear();
        
        // CostsPlot.Plot.Axes.AutoScale();
        // DailyCostsPlot.Plot.Axes.AutoScale();
        // CumulativeDailyCostsPlot.Plot.Axes.AutoScale();
        // CostsPlot.Refresh();
        
        // _skipFirstNReplications = Convert.ToInt32(_viewModel.Replications * (_viewModel.SkipFirstNReplicationsInPercent / 100.0));
        
        //_viewModel.Simulation.SelectedStrategy = _viewModel.SelectedStrategy;
        
        // await Task.Run(() => _viewModel.Simulation.StartSimulation(_viewModel.Replications));
        
        _latestGUIUpdate = -1;
        _skippedGUIUpdates = 0;
        
        _viewModel.TicketStoreSimulation.MaxReplicationTime = _viewModel.MaxReplicationTime;
        _viewModel.TicketStoreSimulation.SimulationSpeed = _viewModel.SpeedOptions.ElementAt(_viewModel.SelectedSpeedIndex).Key;
        await Task.Run(() => _viewModel.TicketStoreSimulation.StartSimulation(_viewModel.Replications));
    }
    
    private void StopSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.TicketStoreSimulation.StopSimulation();
    
    private void PauseResumeSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.PauseResumeSimulation();
    
    private void SpeedOneSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.SetDefaultSpeed();
    
    private void DecreaseSpeedSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.DecreaseSpeed();

    private void IncreaseSpeedSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.IncreaseSpeed();
    
    private void SpeedMaxSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.SetMaxSpeed();

    private void NewReplicationResult(long replication, double costs)
    {
        _viewModel.CurrentReplication = replication.ToString();
        _viewModel.CurrentCosts = costs.ToString("F2");
    }
    
    private void AddValueToChart(long replication, double costs)
    {
        _replicationsChartData.Add(new Coordinates(replication, costs));

        // CostsPlot.Plot.Axes.AutoScale();
        // CostsPlot.Refresh();
    }
    
    private void NewDailyCosts(int day, double dailyCosts, double totalCosts)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _dailyChartData.Add(new Coordinates(day, dailyCosts));
            _cumulativeDailyChartData.Add(new Coordinates(day, totalCosts));
            
            // DailyCostsPlot.Plot.Axes.AutoScale();
            // DailyCostsPlot.Refresh();
            //
            // CumulativeDailyCostsPlot.Plot.Axes.AutoScale();
            // CumulativeDailyCostsPlot.Refresh();
        });
    }
    
    private void SetupCharts()
    {
        // var scatterLineReplications = CostsPlot.Plot.Add.ScatterLine(_replicationsChartData, Colors.Red);
        // scatterLineReplications.PathStrategy = new ScottPlot.PathStrategies.Straight();
        //
        // CostsPlot.Plot.Axes.Bottom.Label.Text = "Replication";
        // CostsPlot.Plot.Axes.Left.Label.Text = "Costs";
        //
        // DailyCostsPlot.Plot.Add.ScatterLine(_dailyChartData, Colors.Green);
        //
        // DailyCostsPlot.Plot.Axes.Bottom.Label.Text = "Day";
        // DailyCostsPlot.Plot.Axes.Left.Label.Text = "Daily costs";
        //
        // var scatterLineCumulativeDaily = CumulativeDailyCostsPlot.Plot.Add.ScatterLine(_cumulativeDailyChartData, Colors.Blue);
        // scatterLineCumulativeDaily.PathStrategy = new ScottPlot.PathStrategies.Straight();
        //
        // CumulativeDailyCostsPlot.Plot.Axes.Bottom.Label.Text = "Day";
        // CumulativeDailyCostsPlot.Plot.Axes.Left.Label.Text = "Cumulative daily costs";
        //
        // CostsPlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);
        // DailyCostsPlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);
        // CumulativeDailyCostsPlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);
    }
}
