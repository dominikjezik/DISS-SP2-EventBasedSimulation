using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DiscreteSimulation.FurnitureManufacturer.DTOs;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using DiscreteSimulation.GUI.ViewModels;
using ScottPlot;
using ScottPlot.AutoScalers;
using ScottPlot.Plottables;

namespace DiscreteSimulation.GUI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    
    private readonly List<Coordinates> _replicationsProcessingOrderTimePlotData = new();
    private readonly List<Coordinates> _replicationsProcessingOrderTimeCILowerPlotData = new();
    private readonly List<Coordinates> _replicationsProcessingOrderTimeCIUpperPlotData = new();

    private Scatter? ScatterLineReplicationsCIUpper;
    private Scatter? ScatterLineReplicationsCILower;
        
    private string _selectedCoordinatesTimeUnit = "seconds";
    private int _skipFirstNReplications = 0;
    private bool _stopSimulationRequested = false;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
        
        SetupCharts();

        _viewModel.Simulation.SimulationStateChanged += SimulationStateChanged;

        _viewModel.Simulation.ReplicationEnded += ReplicationEnded;

        _viewModel.Simulation.SimulationEnded += SimulationEnded;
    }

    private void SimulationEnded()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.EnableButtonsForSimulationEnd();
            SimulationStateChanged();
        });
    }

    private async void StartSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.DisableButtonsForSimulationStart();

        _replicationsProcessingOrderTimePlotData.Clear();
        _replicationsProcessingOrderTimeCILowerPlotData.Clear();
        _replicationsProcessingOrderTimeCIUpperPlotData.Clear();
        
        ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        ProcessingOrderTimePlot.Refresh();
        
        _skipFirstNReplications = Convert.ToInt32(_viewModel.Replications * (_viewModel.SkipFirstNReplicationsInPercent / 100.0));

        
        _viewModel.Simulation.MaxReplicationTime = _viewModel.MaxReplicationTime;
        _viewModel.Simulation.SimulationSpeed = _viewModel.SpeedOptions.ElementAt(_viewModel.SelectedSpeedIndex).Key;
        
        _viewModel.Simulation.CountOfWorkersGroupA = _viewModel.CountOfWorkersGroupA;
        _viewModel.Simulation.CountOfWorkersGroupB = _viewModel.CountOfWorkersGroupB;
        _viewModel.Simulation.CountOfWorkersGroupC = _viewModel.CountOfWorkersGroupC;
        _viewModel.Simulation.EnableWorkerLocationPreference = _viewModel.EnableWorkerLocationPreference;
        
        _latestGUIUpdate = -1;
        _skippedGUIUpdates = 0;

        _viewModel.Orders = new ObservableCollection<OrderDTO>();
        _viewModel.AssemblyLines = new ObservableCollection<AssemblyLineDTO>();
        _viewModel.WorkersGroupA = new ObservableCollection<WorkerDTO>();
        _viewModel.WorkersGroupB = new ObservableCollection<WorkerDTO>();
        _viewModel.WorkersGroupC = new ObservableCollection<WorkerDTO>();
        _viewModel.PendingOrdersQueue = new ObservableCollection<OrderDTO>();
        _viewModel.PendingCutMaterialsQueue = new ObservableCollection<OrderDTO>();
        _viewModel.PendingVarnishedMaterialsQueue = new ObservableCollection<OrderDTO>();
        _viewModel.PendingFoldedClosetsQueue = new ObservableCollection<OrderDTO>();
        _viewModel.SimulationAllWorkersUtilization = new ObservableCollection<WorkerDTO>();
        
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
        
        for (int i = 0; i < _viewModel.Simulation.CountOfWorkersGroupA + _viewModel.Simulation.CountOfWorkersGroupB + _viewModel.Simulation.CountOfWorkersGroupC; i++)
        {
            _viewModel.SimulationAllWorkersUtilization.Add(new WorkerDTO());
        }
        
        await Task.Run(() => _viewModel.Simulation.StartSimulation(_viewModel.Replications));
    }

    private void StopSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _stopSimulationRequested = true;
        _viewModel.Simulation.StopSimulation();
    }
    
    private void PauseResumeSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.PauseResumeSimulation();
    
    private void SpeedOneSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.SetDefaultSpeed();
    
    private void DecreaseSpeedSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.DecreaseSpeed();

    private void IncreaseSpeedSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.IncreaseSpeed();
    
    private void SpeedMaxSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.SetMaxSpeed();

    private double _latestGUIUpdate = -1;
    private int _skippedGUIUpdates = 0;

    private void SimulationStateChanged()
    {
        var simulationTime = _viewModel.Simulation.SimulationTime;
        var simulationSpeed = _viewModel.Simulation.SimulationSpeed;
        var simulationDelta = _viewModel.Simulation.Delta;

        if (!double.IsInfinity(simulationSpeed))
        {
            // Rozdiel posledneho updatu GUI a aktualneho casu simulacie
            var differenceCurrentAndLast = simulationTime - _latestGUIUpdate;

            // Rychlost updatovania GUI je zavisle od delta
            var customParameter = 0.025;
            var minimalDifference = simulationDelta * customParameter;
        
            if (differenceCurrentAndLast < minimalDifference && !_viewModel.Simulation.IsSimulationPaused && _viewModel.Simulation.IsSimulationRunning)
            {
                return;
            }
        }
        else
        {
            // Ak je rychlost nekonecna, updatovanie nemoze byt zavisle od simulačného času pretože, updaty
            // neprichádzajú cca v ekvidistantných časových intervaloch, preto pocitame preskocenie updatov 
            if (_skippedGUIUpdates <= 10_000 && !_viewModel.Simulation.IsSimulationPaused && _viewModel.Simulation.IsSimulationRunning)
            {
                _skippedGUIUpdates++;
                return;
            }
            
            _skippedGUIUpdates = 0;
        }
        
        _latestGUIUpdate = simulationTime;

        // V prípade že simulácia skončila, zobrazíme maximálny čas simulácie.
        // Väčšinou nastane, že ďalšie udalosti sú za hranicou max. času simulácie,
        // čiže čas "nedobehne" na maximálny čas simulácie
        // (aby mal používateľ pocit, že simulácia skutočne bežala až do konca).
        if (!_viewModel.Simulation.IsSimulationRunning && !_stopSimulationRequested)
        {
            simulationTime = _viewModel.Simulation.MaxReplicationTime;
        }
        
        var averageProcessingOrderTime = _viewModel.Simulation.AverageProcessingOrderTime.Mean;
        
        _viewModel.Simulation.PendingOrdersQueue.RefreshStatistics();
        _viewModel.Simulation.PendingCutMaterialsQueue.RefreshStatistics();
        _viewModel.Simulation.PendingVarnishedMaterialsQueue.RefreshStatistics();
        _viewModel.Simulation.PendingFoldedClosetsQueue.RefreshStatistics();
        
        var averagePendingOrdersCount = _viewModel.Simulation.PendingOrdersQueue.AverageQueueLength;
        var averagePendingCutMaterialsCount = _viewModel.Simulation.PendingCutMaterialsQueue.AverageQueueLength;
        var averagePendingVarnishedMaterialsCount = _viewModel.Simulation.PendingVarnishedMaterialsQueue.AverageQueueLength;
        var averagePendingFoldedClosetsCount = _viewModel.Simulation.PendingFoldedClosetsQueue.AverageQueueLength;

        var averageWaitingTimePendingOrders = _viewModel.Simulation.AverageWaitingTimeInPendingOrdersQueue.Mean;
        var averageWaitingTimeCutMaterials = _viewModel.Simulation.AverageWaitingTimeInPendingCutMaterialsQueue.Mean;
        var averageWaitingTimeVarnishedMaterials = _viewModel.Simulation.AverageWaitingTimeInPendingVarnishedMaterialsQueue.Mean;
        var averageWaitingTimeFoldedClosets = _viewModel.Simulation.AverageWaitingTimeInPendingFoldedClosetsQueue.Mean;
        
        var pendingOrdersQueueCount = _viewModel.Simulation.PendingOrdersQueue.Count;
        var pendingCutMaterialsQueueCount = _viewModel.Simulation.PendingCutMaterialsQueue.Count;
        var pendingVarnishedMaterialsQueueCount = _viewModel.Simulation.PendingVarnishedMaterialsQueue.Count;
        var pendingFoldedClosetsQueueCount = _viewModel.Simulation.PendingFoldedClosetsQueue.Count;
        
        // Refresh štatistík vyťaženia pracovníkov
        foreach (Worker worker in _viewModel.Simulation.WorkersGroupA)
        {
            worker.RefreshStatistics();
        }
        
        var workersGroupAUtilization = _viewModel.Simulation.WorkersGroupA.Average(worker => worker.Utilization) * 100;

        foreach (Worker worker in _viewModel.Simulation.WorkersGroupB)
        {
            worker.RefreshStatistics();
        }
        
        var workersGroupBUtilization = _viewModel.Simulation.WorkersGroupB.Average(worker => worker.Utilization) * 100;
        
        foreach (Worker worker in _viewModel.Simulation.WorkersGroupC)
        {
            worker.RefreshStatistics();
        }
        
        var workersGroupCUtilization = _viewModel.Simulation.WorkersGroupC.Average(worker => worker.Utilization) * 100;

        var orders = _viewModel.Simulation.GetCurrentOrderDTOs();
        var pendingOrders = _viewModel.Simulation.GetCurrentPendingOrdersQueue();
        var pendingCutMaterials = _viewModel.Simulation.GetCurrentPendingCutMaterialsQueue();
        var pendingVarnishedMaterials = _viewModel.Simulation.GetCurrentPendingVarnishedMaterialsQueue();
        var pendingFoldedClosets = _viewModel.Simulation.GetCurrentPendingFoldedClosetsQueue();
        var assemblyLines = _viewModel.Simulation.GetCurrentAssemblyLineDTOs();
        var workersGroupA = _viewModel.Simulation.GetCurrentWorkerGroupADTOs();
        var workersGroupB = _viewModel.Simulation.GetCurrentWorkerGroupBDTOs();
        var workersGroupC = _viewModel.Simulation.GetCurrentWorkerGroupCDTOs();
        
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.CurrentSimulationTime = simulationTime.FormatToSimulationTime();
            _viewModel.SetReplicationOrderProcessingTime(averageProcessingOrderTime);
            
            _viewModel.ReplicationPendingOrders = $"{averagePendingOrdersCount:F2}";
            _viewModel.ReplicationPendingCutMaterials = $"{averagePendingCutMaterialsCount:F2}";
            _viewModel.ReplicationPendingVarnishedMaterials = $"{averagePendingVarnishedMaterialsCount:F2}";
            _viewModel.ReplicationPendingFoldedClosets = $"{averagePendingFoldedClosetsCount:F2}";
            
            _viewModel.SetReplicationPendingOrdersWaitingTime(averageWaitingTimePendingOrders);
            _viewModel.SetReplicationCutMaterialsWaitingTime(averageWaitingTimeCutMaterials);
            _viewModel.SetReplicationVarnishedMaterialsWaitingTime(averageWaitingTimeVarnishedMaterials);
            _viewModel.SetReplicationFoldedClosetsWaitingTime(averageWaitingTimeFoldedClosets);
            
            _viewModel.PendingOrdersQueueCount = $"{pendingOrdersQueueCount}";
            _viewModel.PendingCutMaterialsQueueCount = $"{pendingCutMaterialsQueueCount}";
            _viewModel.PendingVarnishedMaterialsQueueCount = $"{pendingVarnishedMaterialsQueueCount}";
            _viewModel.PendingFoldedClosetsQueueCount = $"{pendingFoldedClosetsQueueCount}";
            _viewModel.ReplicationWorkersGroupAUtilization = $"{workersGroupAUtilization:F2}";
            _viewModel.ReplicationWorkersGroupBUtilization = $"{workersGroupBUtilization:F2}";
            _viewModel.ReplicationWorkersGroupCUtilization = $"{workersGroupCUtilization:F2}";
            
            SynchronizeCollection(_viewModel.Orders, orders);
            SynchronizeCollection(_viewModel.AssemblyLines, assemblyLines);
            SynchronizeCollection(_viewModel.WorkersGroupA, workersGroupA);
            SynchronizeCollection(_viewModel.WorkersGroupB, workersGroupB);
            SynchronizeCollection(_viewModel.WorkersGroupC, workersGroupC);
            SynchronizeCollection(_viewModel.PendingOrdersQueue, pendingOrders);
            SynchronizeCollection(_viewModel.PendingCutMaterialsQueue, pendingCutMaterials);
            SynchronizeCollection(_viewModel.PendingVarnishedMaterialsQueue, pendingVarnishedMaterials);
            SynchronizeCollection(_viewModel.PendingFoldedClosetsQueue, pendingFoldedClosets);
        });
    }

    private void ReplicationEnded()
    {
        var currentReplication = _viewModel.Simulation.CurrentReplication;
        
        // V label popiskoch robime refresh bud kazdych 1000 replikacii
        // alebo podla nastavenia RenderOffset respektive poslednu replikaciu
        // (ak module nevyslo na 0) pre aktualnost
        if ((currentReplication % 1000 != 0) && (currentReplication - _skipFirstNReplications) % (_viewModel.RenderOffset + 1) != 0 && currentReplication != _viewModel.Simulation.CurrentMaxReplications)
        {
            return;
        }
        
        var averageProcessingOrderTime = _viewModel.Simulation.SimulationAverageProcessingOrderTime.Mean;
        var averageProcessingOrderTimeCI = _viewModel.Simulation.SimulationAverageProcessingOrderTime.ConfidenceInterval95();
        var averagePendingOrdersCount = _viewModel.Simulation.SimulationAveragePendingOrdersCount.Mean;
        var averagePendingOrdersCountCI = _viewModel.Simulation.SimulationAveragePendingOrdersCount.ConfidenceInterval95();
        var averagePendingCutMaterialsCount = _viewModel.Simulation.SimulationAveragePendingCutMaterialsCount.Mean;
        var averagePendingCutMaterialsCountCI = _viewModel.Simulation.SimulationAveragePendingCutMaterialsCount.ConfidenceInterval95();
        var averagePendingVarnishedMaterialsCount = _viewModel.Simulation.SimulationAveragePendingVarnishedMaterialsCount.Mean;
        var averagePendingVarnishedMaterialsCountCI = _viewModel.Simulation.SimulationAveragePendingVarnishedMaterialsCount.ConfidenceInterval95();
        var averagePendingFoldedClosetsCount = _viewModel.Simulation.SimulationAveragePendingFoldedClosetsCount.Mean;
        var averagePendingFoldedClosetsCountCI = _viewModel.Simulation.SimulationAveragePendingFoldedClosetsCount.ConfidenceInterval95();
        
        var averagePendingOrdersWaitingTime = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingOrdersQueue.Mean;
        var averagePendingOrdersWaitingTimeCI = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingOrdersQueue.ConfidenceInterval95();
        var averageCutMaterialsWaitingTime = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingCutMaterialsQueue.Mean;
        var averageCutMaterialsWaitingTimeCI = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingCutMaterialsQueue.ConfidenceInterval95();
        var averageVarnishedMaterialsWaitingTime = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingVarnishedMaterialsQueue.Mean;
        var averageVarnishedMaterialsWaitingTimeCI = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingVarnishedMaterialsQueue.ConfidenceInterval95();
        var averageFoldedClosetsWaitingTime = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingFoldedClosetsQueue.Mean;
        var averageFoldedClosetsWaitingTimeCI = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingFoldedClosetsQueue.ConfidenceInterval95();
        
        
        var averageWorkersGroupAUtilization = _viewModel.Simulation.SimulationAverageWorkersGroupAUtilization.Mean;
        var averageWorkersGroupAUtilizationCI = _viewModel.Simulation.SimulationAverageWorkersGroupAUtilization.ConfidenceInterval95();
        var averageWorkersGroupBUtilization = _viewModel.Simulation.SimulationAverageWorkersGroupBUtilization.Mean;
        var averageWorkersGroupBUtilizationCI = _viewModel.Simulation.SimulationAverageWorkersGroupBUtilization.ConfidenceInterval95();
        var averageWorkersGroupCUtilization = _viewModel.Simulation.SimulationAverageWorkersGroupCUtilization.Mean;
        var averageWorkersGroupCUtilizationCI = _viewModel.Simulation.SimulationAverageWorkersGroupCUtilization.ConfidenceInterval95();

        var allWorkersUtilization = _viewModel.Simulation.GetAllWorkersSimulationUtilization();
        
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.CurrentReplication = currentReplication.ToString();
            _viewModel.SetSimulationCurrentProcessingOrderTime(averageProcessingOrderTime, averageProcessingOrderTimeCI.Item1, averageProcessingOrderTimeCI.Item2);
            
            _viewModel.SimulationPendingOrders = $"{averagePendingOrdersCount:F2}";
            _viewModel.SimulationPendingOrdersCI = $"<{averagePendingOrdersCountCI.Item1:F2} ; {averagePendingOrdersCountCI.Item2:F2}>";
            _viewModel.SimulationPendingCutMaterials = $"{averagePendingCutMaterialsCount:F2}";
            _viewModel.SimulationPendingCutMaterialsCI = $"<{averagePendingCutMaterialsCountCI.Item1:F2} ; {averagePendingCutMaterialsCountCI.Item2:F2}>";
            _viewModel.SimulationPendingVarnishedMaterials = $"{averagePendingVarnishedMaterialsCount:F2}";
            _viewModel.SimulationPendingVarnishedMaterialsCI = $"<{averagePendingVarnishedMaterialsCountCI.Item1:F2} ; {averagePendingVarnishedMaterialsCountCI.Item2:F2}>";
            _viewModel.SimulationPendingFoldedClosets = $"{averagePendingFoldedClosetsCount:F2}";
            _viewModel.SimulationPendingFoldedClosetsCI = $"<{averagePendingFoldedClosetsCountCI.Item1:F2} ; {averagePendingFoldedClosetsCountCI.Item2:F2}>";
            
            _viewModel.SetSimulationPendingOrdersWaitingTime(
                averagePendingOrdersWaitingTime, 
                averagePendingOrdersWaitingTimeCI.Item1, 
                averagePendingOrdersWaitingTimeCI.Item2
            );
            _viewModel.SetSimulationCutMaterialsWaitingTime(
                averageCutMaterialsWaitingTime, 
                averageCutMaterialsWaitingTimeCI.Item1, 
                averageCutMaterialsWaitingTimeCI.Item2
            );
            _viewModel.SetSimulationVarnishedMaterialsWaitingTime(
                averageVarnishedMaterialsWaitingTime, 
                averageVarnishedMaterialsWaitingTimeCI.Item1, 
                averageVarnishedMaterialsWaitingTimeCI.Item2
            );
            _viewModel.SetSimulationFoldedClosetsWaitingTime(
                averageFoldedClosetsWaitingTime, 
                averageFoldedClosetsWaitingTimeCI.Item1, 
                averageFoldedClosetsWaitingTimeCI.Item2
            );
            
            _viewModel.SimulationWorkersAUtilization = $"{averageWorkersGroupAUtilization * 100:F2}";
            _viewModel.SimulationWorkersAUtilizationCI = $"<{(averageWorkersGroupAUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupAUtilizationCI.Item2*100):F2}>";
            _viewModel.SimulationWorkersBUtilization = $"{averageWorkersGroupBUtilization * 100:F2}";
            _viewModel.SimulationWorkersBUtilizationCI = $"<{(averageWorkersGroupBUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupBUtilizationCI.Item2*100):F2}>";
            _viewModel.SimulationWorkersCUtilization = $"{averageWorkersGroupCUtilization * 100:F2}";
            _viewModel.SimulationWorkersCUtilizationCI = $"<{(averageWorkersGroupCUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupCUtilizationCI.Item2*100):F2}>";
            
            for (int i = 0; i < allWorkersUtilization.Count; i++)
            {
                _viewModel.SimulationAllWorkersUtilization[i].Id = allWorkersUtilization[i].Id;
                _viewModel.SimulationAllWorkersUtilization[i].State = allWorkersUtilization[i].State;
                _viewModel.SimulationAllWorkersUtilization[i].Utilization = allWorkersUtilization[i].Utilization;
                _viewModel.SimulationAllWorkersUtilization[i].Place = allWorkersUtilization[i].Place;
            }
        });
        
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

        Dispatcher.UIThread.Post(() => AddValueToProcessingOrderTimePlot(
            currentReplication,
            averageProcessingOrderTime,
            averageProcessingOrderTimeCI.Item1,
            averageProcessingOrderTimeCI.Item2
        ));
    }

    private void SynchronizeCollection<TItem>(ObservableCollection<TItem> targetList, List<TItem> sourceList) where TItem : IUpdatable<TItem>
    {
        // Aktualizácia položiek
        for (int i = 0; i < targetList.Count && i < sourceList.Count; i++)
        {
            targetList[i].Update(sourceList[i]);
        }
        
        // Pridanie chýbajúcich položiek v kolekcii
        for (int i = targetList.Count; i < sourceList.Count; i++)
        {
            var item = Activator.CreateInstance<TItem>();
            item.Update(sourceList[i]);
            targetList.Add(item);
        }
        
        // Odstránenie položiek v kolekcii navyše
        for (int i = targetList.Count - 1; i >= sourceList.Count; i--)
        {
            targetList.RemoveAt(i);
        }
    }
    
    private void AddValueToProcessingOrderTimePlot(long replication, double processingOrderTime, double timeLower, double timeUpper)
    {
        var newY = RecalculateCoordinateValue(processingOrderTime, "seconds");
        var newYLower = RecalculateCoordinateValue(timeLower, "seconds");
        var newYUpper = RecalculateCoordinateValue(timeUpper, "seconds");
        
        _replicationsProcessingOrderTimePlotData.Add(new Coordinates(replication, newY));
        _replicationsProcessingOrderTimeCILowerPlotData.Add(new Coordinates(replication, newYLower));
        _replicationsProcessingOrderTimeCIUpperPlotData.Add(new Coordinates(replication, newYUpper));

        ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        ProcessingOrderTimePlot.Refresh();
    }
    
    private void SetupCharts()
    {
        var scatterLineReplications = ProcessingOrderTimePlot.Plot.Add.ScatterLine(_replicationsProcessingOrderTimePlotData, Colors.Red);
        scatterLineReplications.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        ScatterLineReplicationsCIUpper = ProcessingOrderTimePlot.Plot.Add.ScatterLine(_replicationsProcessingOrderTimeCIUpperPlotData, Colors.Gray);
        ScatterLineReplicationsCIUpper.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        ScatterLineReplicationsCILower = ProcessingOrderTimePlot.Plot.Add.ScatterLine(_replicationsProcessingOrderTimeCILowerPlotData, Colors.Gray);
        ScatterLineReplicationsCILower.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        ProcessingOrderTimePlot.Plot.Axes.Bottom.Label.Text = "Replication";
        ProcessingOrderTimePlot.Plot.Axes.Left.Label.Text = "Processing order time";

        ProcessingOrderTimePlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);
    }

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;
        _viewModel.SelectedTimeUnits = menuItem.Header.ToString();
        
        var newData = new List<Coordinates>();
        
        // Replication data
        foreach (var coordinates in _replicationsProcessingOrderTimePlotData)
        {
            var newX = coordinates.X;
            var newY = RecalculateCoordinateValue(coordinates.Y);
            
            newData.Add(new Coordinates(newX, newY));
        }
        
        _replicationsProcessingOrderTimePlotData.Clear();
        
        foreach (var coordinates in newData)
        {
            _replicationsProcessingOrderTimePlotData.Add(new Coordinates(coordinates.X, coordinates.Y));
        }
        
        // Lower CI data
        newData.Clear();
        
        foreach (var coordinates in _replicationsProcessingOrderTimeCILowerPlotData)
        {
            var newX = coordinates.X;
            var newY = RecalculateCoordinateValue(coordinates.Y);
            
            newData.Add(new Coordinates(newX, newY));
        }
        
        _replicationsProcessingOrderTimeCILowerPlotData.Clear();
        
        foreach (var coordinates in newData)
        {
            _replicationsProcessingOrderTimeCILowerPlotData.Add(new Coordinates(coordinates.X, coordinates.Y));
        }
        
        // Upper CI data
        newData.Clear();
        
        foreach (var coordinates in _replicationsProcessingOrderTimeCIUpperPlotData)
        {
            var newX = coordinates.X;
            var newY = RecalculateCoordinateValue(coordinates.Y);
            
            newData.Add(new Coordinates(newX, newY));
        }
        
        _replicationsProcessingOrderTimeCIUpperPlotData.Clear();
        
        foreach (var coordinates in newData)
        {
            _replicationsProcessingOrderTimeCIUpperPlotData.Add(new Coordinates(coordinates.X, coordinates.Y));
        }
        
        _selectedCoordinatesTimeUnit = _viewModel.SelectedTimeUnits;
        
        ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        ProcessingOrderTimePlot.Refresh();
    }
    
    private double RecalculateCoordinateValue(double coordinate, string? currentTimeUnit = null)
    {
        if (currentTimeUnit == null)
        {
            currentTimeUnit = _selectedCoordinatesTimeUnit;
        }
        
        return currentTimeUnit switch
        {
            "seconds" => _viewModel.SelectedTimeUnits switch
            {
                "seconds" => coordinate,
                "minutes" => coordinate / 60,
                "hours" => coordinate / 3600,
                _ => coordinate
            },
            "minutes" => _viewModel.SelectedTimeUnits switch
            {
                "seconds" => coordinate * 60,
                "minutes" => coordinate,
                "hours" => coordinate / 60,
                _ => coordinate
            },
            "hours" => _viewModel.SelectedTimeUnits switch
            {
                "seconds" => coordinate * 3600,
                "minutes" => coordinate * 60,
                "hours" => coordinate,
                _ => coordinate
            },
        };
    }

    private void Render95ConfidenceIntervalCheckbox_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        
        if (ScatterLineReplicationsCIUpper == null || ScatterLineReplicationsCILower == null)
        {
            return;
        }
        
        ScatterLineReplicationsCIUpper.IsVisible = checkBox.IsChecked == true;
        ScatterLineReplicationsCILower.IsVisible = checkBox.IsChecked == true;
        
        ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        ProcessingOrderTimePlot.Refresh();
    }
}
