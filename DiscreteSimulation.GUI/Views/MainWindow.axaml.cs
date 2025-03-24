using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DiscreteSimulation.FurnitureManufacturer.DTOs;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
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

        _viewModel.Simulation.SimulationStateChanged += SimulationStateChanged;
        //_viewModel.TicketStoreSimulation.SimulationStateChanged += SimulationStateChanged;
        
        //_viewModel.Simulation.SimulationStateChanged += SimulationStateChanged;
        // _viewModel.TicketStoreSimulation.ReplicationEnded += ReplicationEnded;

        _viewModel.Simulation.SimulationEnded += () =>
        {
            Dispatcher.UIThread.Post(() => _viewModel.EnableButtonsForSimulationEnd());
        };
        //_viewModel.TicketStoreSimulation.SimulationEnded += () =>
        //{
        //    Dispatcher.UIThread.Post(() => _viewModel.EnableButtonsForSimulationEnd());
        //};
    }

    private double _latestGUIUpdate = -1;
    private int _skippedGUIUpdates = 0;

    private void SimulationStateChanged()
    {
        var simulationTime = _viewModel.Simulation.SimulationTime;
        var simulationSpeed = _viewModel.Simulation.SimulationSpeed;

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
        
        //var averageWaitingTime = _viewModel.Simulation.AverageWaitingTime.Mean;
        //var averageQueueLength = _viewModel.Simulation.CustomersQueue.AverageQueueLength;
        
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.SetCurrentSimulationTime(simulationTime);
            
            
            //_viewModel.AvgOrderProcessingTime = averageWaitingTime.ToString("F2");
            //_viewModel.AvgPendingOrders = averageQueueLength.ToString("F2");
            
            
            // TODO: Zmena môže byť desynchronizovaná lebo dáta získavam v UITrea
            // Bude treba vlastnú implementáciu s manuálnym refreshom
            
            // ORDERS
            
            // Aktualizácia položiek objednávok
            for (int i = 0; i < _viewModel.Orders.Count && i < _viewModel.Simulation.UnfinishedOrders.Count; i++)
            {
                _viewModel.Orders[i].Update(_viewModel.Simulation.UnfinishedOrders[i]);
            }
            
            // Pridanie chýbajúcich položiek v tabuľke
            for (int i = _viewModel.Orders.Count; i < _viewModel.Simulation.UnfinishedOrders.Count; i++)
            {
                var orderDto = new OrderDTO();
                orderDto.Update(_viewModel.Simulation.UnfinishedOrders[i]);
                _viewModel.Orders.Add(orderDto);
            }

            // Odstránenie položiek v tabuľke navyše
            for (int i = _viewModel.Orders.Count - 1; i >= _viewModel.Simulation.UnfinishedOrders.Count; i--)
            {
                _viewModel.Orders.RemoveAt(i);
            }
            
            // ASSERMBLY LINES
            
            // Aktualizácia položiek linky
            for (int i = 0; i < _viewModel.AssemblyLines.Count && i < _viewModel.Simulation.AssemblyLines.Count; i++)
            {
                _viewModel.AssemblyLines[i].Update(_viewModel.Simulation.AssemblyLines[i]);
            }
            
            // Pridanie chýbajúcich položiek v tabuľke
            for (int i = _viewModel.AssemblyLines.Count; i < _viewModel.Simulation.AssemblyLines.Count; i++)
            {
                var assemblyLineDto = new AssemblyLineDTO();
                assemblyLineDto.Update(_viewModel.Simulation.AssemblyLines[i]);
                _viewModel.AssemblyLines.Add(assemblyLineDto);
            }
            
            // Odstránenie položiek v tabuľke navyše
            for (int i = _viewModel.AssemblyLines.Count - 1; i >= _viewModel.Simulation.AssemblyLines.Count; i--)
            {
                _viewModel.AssemblyLines.RemoveAt(i);
            }
            
            // WORKERS GROUP A
            for (int i = 0; i < _viewModel.WorkersGroupA.Count && i < _viewModel.Simulation.WorkersGroupA.Length; i++)
            {
                _viewModel.WorkersGroupA[i].Update(_viewModel.Simulation.WorkersGroupA[i], WorkerGroup.GroupA);
            }
            
            // WORKERS GROUP B
            for (int i = 0; i < _viewModel.WorkersGroupB.Count && i < _viewModel.Simulation.WorkersGroupB.Length; i++)
            {
                _viewModel.WorkersGroupB[i].Update(_viewModel.Simulation.WorkersGroupB[i], WorkerGroup.GroupB);
            }
            
            // WORKERS GROUP C
            for (int i = 0; i < _viewModel.WorkersGroupC.Count && i < _viewModel.Simulation.WorkersGroupC.Length; i++)
            {
                _viewModel.WorkersGroupC[i].Update(_viewModel.Simulation.WorkersGroupC[i], WorkerGroup.GroupC);
            }
            
            

        });
        
        /*
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
        */
    }

    private void ReplicationEnded()
    {
        if (_viewModel.WarehouseSimulation.CurrentMaxReplications == 1)
        {
            var currentCostsForSingleReplication = _viewModel.WarehouseSimulation.CurrentCosts;
            Dispatcher.UIThread.Post(() => NewReplicationResult(1, currentCostsForSingleReplication));
            return;
        }
        
        var currentReplication = _viewModel.WarehouseSimulation.CurrentReplication;
        
        // V label popiskoch robime refresh bud kazdych 1000 replikacii
        // alebo podla nastavenia RenderOffset respektive poslednu replikaciu
        // (ak module nevyslo na 0) pre aktualnost
        if ((currentReplication % 1000 != 0) && (currentReplication - _skipFirstNReplications) % (_viewModel.RenderOffset + 1) != 0
            && currentReplication != _viewModel.WarehouseSimulation.CurrentMaxReplications)
        {
            return;
        }
        
        var currentCosts = _viewModel.WarehouseSimulation.CurrentCosts;
        
        Dispatcher.UIThread.Post(() => NewReplicationResult(currentReplication, currentCosts));
        
        // Berieme iba kazdu (RenderOffset + 1)-tu replikaciu pre vykreslenie
        // Ale ak sme uz na poslednej replikacii, a modulo nevyslo na 0,
        // tak sa zobrazi aj tak
        if ((currentReplication - _skipFirstNReplications) % (_viewModel.RenderOffset + 1) != 0 
            && currentReplication != _viewModel.WarehouseSimulation.CurrentMaxReplications)
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
        
        
        
        _viewModel.Simulation.MaxReplicationTime = _viewModel.MaxReplicationTime;
        _viewModel.Simulation.SimulationSpeed = _viewModel.SpeedOptions.ElementAt(_viewModel.SelectedSpeedIndex).Key;
        
        // TODO: Vytiahnúť počty pracovníkov na GUI
        _viewModel.Simulation.CountOfWorkersGroupA = 5;
        _viewModel.Simulation.CountOfWorkersGroupB = 2;
        _viewModel.Simulation.CountOfWorkersGroupC = 10;
        
        _latestGUIUpdate = -1;
        _skippedGUIUpdates = 0;

        _viewModel.Orders = new ObservableCollection<OrderDTO>();
        _viewModel.WorkersGroupA = new ObservableCollection<WorkerDTO>();
        _viewModel.WorkersGroupB = new ObservableCollection<WorkerDTO>();
        _viewModel.WorkersGroupC = new ObservableCollection<WorkerDTO>();
        
        for (int i = 0; i < _viewModel.Simulation.CountOfWorkersGroupA; i++)
        {
            _viewModel.WorkersGroupA.Add(new WorkerDTO());
        }
        
        for (int i = 0; i < _viewModel.Simulation.CountOfWorkersGroupB; i++)
        {
            _viewModel.WorkersGroupB.Add(new WorkerDTO());
        }
        
        for (int i = 0; i < _viewModel.Simulation.CountOfWorkersGroupC; i++)
        {
            _viewModel.WorkersGroupC.Add(new WorkerDTO());
        }
        
        await Task.Run(() => _viewModel.Simulation.StartSimulation(_viewModel.Replications));
        
        /*
        _viewModel.TicketStoreSimulation.MaxReplicationTime = _viewModel.MaxReplicationTime;
        _viewModel.TicketStoreSimulation.SimulationSpeed = _viewModel.SpeedOptions.ElementAt(_viewModel.SelectedSpeedIndex).Key;
        await Task.Run(() => _viewModel.TicketStoreSimulation.StartSimulation(_viewModel.Replications));
        */
    }
    
    private void StopSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.Simulation.StopSimulation();
    
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
