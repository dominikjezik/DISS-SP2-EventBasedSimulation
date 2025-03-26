using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DiscreteSimulation.FurnitureManufacturer.DTOs;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.GUI.ViewModels;
using ScottPlot;
using ScottPlot.AutoScalers;

namespace DiscreteSimulation.GUI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    
    private readonly List<Coordinates> _replicationsProcessingOrderTimePlotData = new();
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
        
        ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        ProcessingOrderTimePlot.Refresh();
        
        _skipFirstNReplications = Convert.ToInt32(_viewModel.Replications * (_viewModel.SkipFirstNReplicationsInPercent / 100.0));

        
        _viewModel.Simulation.MaxReplicationTime = _viewModel.MaxReplicationTime;
        _viewModel.Simulation.SimulationSpeed = _viewModel.SpeedOptions.ElementAt(_viewModel.SelectedSpeedIndex).Key;
        
        _viewModel.Simulation.CountOfWorkersGroupA = _viewModel.CountOfWorkersGroupA;
        _viewModel.Simulation.CountOfWorkersGroupB = _viewModel.CountOfWorkersGroupB;
        _viewModel.Simulation.CountOfWorkersGroupC = _viewModel.CountOfWorkersGroupC;
        
        _latestGUIUpdate = -1;
        _skippedGUIUpdates = 0;

        _viewModel.Orders = new ObservableCollection<OrderDTO>();
        _viewModel.AssemblyLines = new ObservableCollection<AssemblyLineDTO>();
        _viewModel.WorkersGroupA = new ObservableCollection<WorkerDTO>();
        _viewModel.WorkersGroupB = new ObservableCollection<WorkerDTO>();
        _viewModel.WorkersGroupC = new ObservableCollection<WorkerDTO>();
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
        var averagePendingOrdersCount = _viewModel.Simulation.PendingOrdersQueue.AverageQueueLength;
        
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
        var assemblyLines = _viewModel.Simulation.GetCurrentAssemblyLineDTOs();
        var workersGroupA = _viewModel.Simulation.GetCurrentWorkerGroupADTOs();
        var workersGroupB = _viewModel.Simulation.GetCurrentWorkerGroupBDTOs();
        var workersGroupC = _viewModel.Simulation.GetCurrentWorkerGroupCDTOs();
        
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.SetCurrentSimulationTime(simulationTime);
            _viewModel.ReplicationOrderProcessingTime = $"{averageProcessingOrderTime:F2}";
            _viewModel.ReplicationPendingOrders = $"{averagePendingOrdersCount:F2}";
            _viewModel.PendingOrdersQueueCount = $"{pendingOrdersQueueCount}";
            _viewModel.PendingCutMaterialsQueueCount = $"{pendingCutMaterialsQueueCount}";
            _viewModel.PendingVarnishedMaterialsQueueCount = $"{pendingVarnishedMaterialsQueueCount}";
            _viewModel.PendingFoldedClosetsQueueCount = $"{pendingFoldedClosetsQueueCount}";
            _viewModel.ReplicationWorkersGroupAUtilization = $"({workersGroupAUtilization:F2} %)";
            _viewModel.ReplicationWorkersGroupBUtilization = $"({workersGroupBUtilization:F2} %)";
            _viewModel.ReplicationWorkersGroupCUtilization = $"({workersGroupCUtilization:F2} %)";
            
            UpdateOrdersList(orders);
            UpdateAssemblyLinesList(assemblyLines);
            UpdateWorkersLists(workersGroupA, workersGroupB, workersGroupC);
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
        var averagePendingOrdersCountCI = _viewModel.Simulation.SimulationAveragePendingOrdersCount.ConfidenceInterval95();
        var averageWorkersGroupAUtilizationCI = _viewModel.Simulation.SimulationAverageWorkersGroupAUtilization.ConfidenceInterval95();
        var averageWorkersGroupBUtilizationCI = _viewModel.Simulation.SimulationAverageWorkersGroupBUtilization.ConfidenceInterval95();
        var averageWorkersGroupCUtilizationCI = _viewModel.Simulation.SimulationAverageWorkersGroupCUtilization.ConfidenceInterval95();

        var allWorkersUtilization = _viewModel.Simulation.GetAllWorkersSimulationUtilization();
        
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.CurrentReplication = currentReplication.ToString();
            _viewModel.SimulationCurrentProcessingOrderTimeSeconds = $"<{averageProcessingOrderTimeCI.Item1:F2} ; {averageProcessingOrderTimeCI.Item2:F2}>";
            _viewModel.SimulationCurrentProcessingOrderTimeMinutes = $"<{averageProcessingOrderTimeCI.Item1 / 60:F2} ; {averageProcessingOrderTimeCI.Item2 / 60:F2}>";
            _viewModel.SimulationCurrentProcessingOrderTimeHours = $"<{averageProcessingOrderTimeCI.Item1 / 3600:F2} ; {averageProcessingOrderTimeCI.Item2 / 3600:F2}>";
            _viewModel.SimulationPendingOrders = $"<{averagePendingOrdersCountCI.Item1:F2} ; {averagePendingOrdersCountCI.Item2:F2}>";
            _viewModel.SimulationWorkersAUtilization = $"<{(averageWorkersGroupAUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupAUtilizationCI.Item2*100):F2}>";
            _viewModel.SimulationWorkersBUtilization = $"<{(averageWorkersGroupBUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupBUtilizationCI.Item2*100):F2}>";
            _viewModel.SimulationWorkersCUtilization = $"<{(averageWorkersGroupCUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupCUtilizationCI.Item2*100):F2}>";
            
            for (int i = 0; i < allWorkersUtilization.Count; i++)
            {
                _viewModel.SimulationAllWorkersUtilization[i].Id = allWorkersUtilization[i].Id;
                _viewModel.SimulationAllWorkersUtilization[i].State = allWorkersUtilization[i].State;
                _viewModel.SimulationAllWorkersUtilization[i].Utilization = allWorkersUtilization[i].Utilization;
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
        
        Dispatcher.UIThread.Post(() => AddValueToProcessingOrderTimePlot(currentReplication, averageProcessingOrderTime));
    }
    
    public void UpdateOrdersList(List<OrderDTO> orders)
    {
        // Aktualizácia položiek objednávok
        for (int i = 0; i < _viewModel.Orders.Count && i < orders.Count; i++)
        {
            _viewModel.Orders[i].Update(orders[i]);
        }
        
        // Pridanie chýbajúcich položiek v tabuľke
        for (int i = _viewModel.Orders.Count; i < orders.Count; i++)
        {
            var orderDto = new OrderDTO();
            orderDto.Update(orders[i]);
            _viewModel.Orders.Add(orderDto);
        }

        // Odstránenie položiek v tabuľke navyše
        for (int i = _viewModel.Orders.Count - 1; i >= orders.Count; i--)
        {
            _viewModel.Orders.RemoveAt(i);
        }
    }
    
    public void UpdateAssemblyLinesList(List<AssemblyLineDTO> assemblyLines)
    {
        // Aktualizácia položiek linky
        for (int i = 0; i < _viewModel.AssemblyLines.Count && i < assemblyLines.Count; i++)
        {
            _viewModel.AssemblyLines[i].Update(assemblyLines[i]);
        }
        
        // Pridanie chýbajúcich položiek v tabuľke
        for (int i = _viewModel.AssemblyLines.Count; i < assemblyLines.Count; i++)
        {
            var assemblyLineDto = new AssemblyLineDTO();
            assemblyLineDto.Update(assemblyLines[i]);
            _viewModel.AssemblyLines.Add(assemblyLineDto);
        }
        
        // Odstránenie položiek v tabuľke navyše
        for (int i = _viewModel.AssemblyLines.Count - 1; i >= assemblyLines.Count; i--)
        {
            _viewModel.AssemblyLines.RemoveAt(i);
        }
    }

    public void UpdateWorkersLists(List<WorkerDTO> workersGroupA, List<WorkerDTO> workersGroupB, List<WorkerDTO> workersGroupC)
    {
        // Aktualizácia položiek pracovníkov skupiny A
        for (int i = 0; i < _viewModel.WorkersGroupA.Count && i < workersGroupA.Count; i++)
        {
            _viewModel.WorkersGroupA[i].Update(workersGroupA[i]);
        }
        
        // Aktualizácia položiek pracovníkov skupiny B
        for (int i = 0; i < _viewModel.WorkersGroupB.Count && i < workersGroupB.Count; i++)
        {
            _viewModel.WorkersGroupB[i].Update(workersGroupB[i]);
        }
        
        // Aktualizácia položiek pracovníkov skupiny C
        for (int i = 0; i < _viewModel.WorkersGroupC.Count && i < workersGroupC.Count; i++)
        {
            _viewModel.WorkersGroupC[i].Update(workersGroupC[i]);
        }
    }
    
    private void AddValueToProcessingOrderTimePlot(long replication, double processingOrderTime)
    {
        _replicationsProcessingOrderTimePlotData.Add(new Coordinates(replication, processingOrderTime));

        ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        ProcessingOrderTimePlot.Refresh();
    }
    
    private void SetupCharts()
    {
        var scatterLineReplications = ProcessingOrderTimePlot.Plot.Add.ScatterLine(_replicationsProcessingOrderTimePlotData, Colors.Red);
        scatterLineReplications.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        ProcessingOrderTimePlot.Plot.Axes.Bottom.Label.Text = "Replication";
        ProcessingOrderTimePlot.Plot.Axes.Left.Label.Text = "Processing order time";

        ProcessingOrderTimePlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);
    }
}
