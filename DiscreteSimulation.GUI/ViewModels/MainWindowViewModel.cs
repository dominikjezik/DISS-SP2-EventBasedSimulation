using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DiscreteSimulation.FurnitureManufacturer.DTOs;
using DiscreteSimulation.FurnitureManufacturer.Simulation;
using DiscreteSimulation.FurnitureManufacturer.TicketStore;
using DiscreteSimulation.FurnitureManufacturer.Warehouse;

namespace DiscreteSimulation.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FurnitureManufacturerSimulation _simulation = new();
    
    private readonly TicketStoreSimulation _ticketStoreSimulation = new();
    
    private readonly WarehouseSimulation _warehouseSimulation = new();
    
    public FurnitureManufacturerSimulation Simulation => _simulation;
    
    public WarehouseSimulation WarehouseSimulation => _warehouseSimulation;
    
    public TicketStoreSimulation TicketStoreSimulation => _ticketStoreSimulation;
    
    
    #region SimulationControlButtons
    
    private bool _isStartSimulationButtonEnabled = true;

    public bool IsStartSimulationButtonEnabled
    {
        get => _isStartSimulationButtonEnabled;
        set
        {
            _isStartSimulationButtonEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsStopSimulationButtonEnabled));
            OnPropertyChanged(nameof(IsSpeedSelectorEnabled));
            OnPropertyChanged(nameof(IsDefaultSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsDecreaseSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsIncreaseSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsSpeedMaxButtonEnabled));
        }
    }
    
    public bool IsStopSimulationButtonEnabled => !IsStartSimulationButtonEnabled;

    private bool _isPauseResumeSimulationButtonEnabled = false;
    
    public bool IsPauseResumeSimulationButtonEnabled
    {
        get => _isPauseResumeSimulationButtonEnabled;
        set
        {
            _isPauseResumeSimulationButtonEnabled = value;
            OnPropertyChanged();
        }
    }
    
    private string _pauseResumeSimulationButtonText = "Pause";
    
    public string PauseResumeSimulationButtonText
    {
        get => _pauseResumeSimulationButtonText;
        set
        {
            _pauseResumeSimulationButtonText = value;
            OnPropertyChanged();
        }
    }

    public void PauseResumeSimulation()
    {
        if (Simulation.IsSimulationPaused)
        {
            Simulation.ResumeSimulation();
            PauseResumeSimulationButtonText = "Pause";
        }
        else
        {
            Simulation.PauseSimulation();
            PauseResumeSimulationButtonText = "Resume";
        }
        
        /*
        if (TicketStoreSimulation.IsSimulationPaused)
        {
            TicketStoreSimulation.ResumeSimulation();
            PauseResumeSimulationButtonText = "Pause";
        }
        else
        {
            TicketStoreSimulation.PauseSimulation();
            PauseResumeSimulationButtonText = "Resume";
        }
        */
    }
    
    public bool IsDefaultSpeedButtonEnabled => SelectedSpeedIndex != 0 && (_isStartSimulationButtonEnabled || _selectedSpeedIndex != SpeedOptions.Count - 1);
    
    public bool IsDecreaseSpeedButtonEnabled => SelectedSpeedIndex > 0 && (_isStartSimulationButtonEnabled || _selectedSpeedIndex != SpeedOptions.Count - 1);
    
    public bool IsIncreaseSpeedButtonEnabled => SelectedSpeedIndex < SpeedOptions.Count - 1 && (_isStartSimulationButtonEnabled || _selectedSpeedIndex != SpeedOptions.Count - 1);
    
    public bool IsSpeedMaxButtonEnabled => SelectedSpeedIndex < SpeedOptions.Count - 1 && (_isStartSimulationButtonEnabled || _selectedSpeedIndex != SpeedOptions.Count - 1);
    
    public OrderedDictionary<double, string> SpeedOptions { get; set; } = new()
    {
        { 1.0, "x1" },
        { 2.0, "x2" },
        { 5.0, "x5" },
        { 10.0, "x10" },
        { 50.0, "x50" },
        { 100.0, "x100" },
        { 500.0, "x500" },
        { 1000.0, "x1000" },
        { 10000.0, "x10000" },
        { 100000.0, "x100000" },
        { double.PositiveInfinity, "MAX" }
    };

    private int _selectedSpeedIndex = 0;

    public int SelectedSpeedIndex
    {
        get => _selectedSpeedIndex;
        set
        {
            _selectedSpeedIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDefaultSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsDecreaseSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsIncreaseSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsSpeedMaxButtonEnabled));
            OnPropertyChanged(nameof(IsSpeedSelectorEnabled));
            
            Simulation.SimulationSpeed = SpeedOptions.ElementAt(SelectedSpeedIndex).Key;
            //TicketStoreSimulation.SimulationSpeed = SpeedOptions.ElementAt(SelectedSpeedIndex).Key;
        }
    }

    public bool IsSpeedSelectorEnabled => _isStartSimulationButtonEnabled || _selectedSpeedIndex != SpeedOptions.Count - 1;
    
    public void DecreaseSpeed()
    {
        if (SelectedSpeedIndex == 0)
        {
            return;
        }
        
        SelectedSpeedIndex--;
    }

    public void IncreaseSpeed()
    {
        SelectedSpeedIndex++;
    }

    public void SetMaxSpeed()
    {
        SelectedSpeedIndex = SpeedOptions.Count - 1;
    }
    
    public void SetDefaultSpeed()
    {
        SelectedSpeedIndex = 0;
    }

    public void DisableButtonsForSimulationStart()
    {
        IsStartSimulationButtonEnabled = false;
        IsPauseResumeSimulationButtonEnabled = true;
    }

    public void EnableButtonsForSimulationEnd()
    {
        IsStartSimulationButtonEnabled = true;
        IsPauseResumeSimulationButtonEnabled = false;
    }
    
    #endregion

    #region ReplicationControls

    private long _replications = 1;
    
    public long Replications
    {
        get => _replications;
        set
        {
            if (value == _replications) return;
            _replications = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RenderOffset));

            if (value < RenderPoints)
            {
                RenderPoints = value;
            }
            
            if (Replications == 1)
            {
                IsSingleReplication = true;
            }
            else
            {
                IsSingleReplication = false;
            }
        }
    }
    
    private bool _isSingleReplication = true;
    
    public bool IsSingleReplication
    {
        get => _isSingleReplication;
        set
        {
            _isSingleReplication = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsMultipleReplications));
        }
    }
    
    public bool IsMultipleReplications => !IsSingleReplication;
    
    private int _maxReplicationTime = 7_171_200;
    
    public int MaxReplicationTime
    {
        get => _maxReplicationTime;
        set
        {
            _maxReplicationTime = value;
            OnPropertyChanged();
        }
    }
    
    #endregion
    
    #region SingleReplicationControls
    
    private string _avgOrderProcessingTime = "-";

    public string AvgOrderProcessingTime
    {
        get => _avgOrderProcessingTime;
        set
        {
            _avgOrderProcessingTime = value;
            OnPropertyChanged();
        }
    }
    
    private string _avgPendingOrders = "-";
    
    public string AvgPendingOrders
    {
        get => _avgPendingOrders;
        set
        {
            _avgPendingOrders = value;
            OnPropertyChanged();
        }
    }

    private string _currentSimulationTime = "[Day 1] 06:00:00";
    
    public string CurrentSimulationTime
    {
        get => _currentSimulationTime;
        set
        {
            _currentSimulationTime = value;
            OnPropertyChanged();
        }
    }

    public void SetCurrentSimulationTime(double simulationTime)
    {
        var workingDays = Math.Floor(simulationTime / 28_800);
        simulationTime -= workingDays * 28_800;
        
        var hours = Math.Floor(simulationTime / 3_600);
        simulationTime -= hours * 3_600;
        
        var minutes = Math.Floor(simulationTime / 60);
        simulationTime -= minutes * 60;
        
        var seconds = Math.Floor(simulationTime);
        simulationTime -= seconds;
        
        hours += 6;

        CurrentSimulationTime = $"[Day {workingDays + 1}] {hours:00}:{minutes:00}:{seconds:00}";
    }

    private ObservableCollection<OrderDTO> _orders = [];

    public ObservableCollection<OrderDTO> Orders
    {
        get => _orders;
        set {
            _orders = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<AssemblyLineDTO> _assemblyLines = [];
    
    public ObservableCollection<AssemblyLineDTO> AssemblyLines
    {
        get => _assemblyLines;
        set {
            _assemblyLines = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<WorkerDTO> _workersGroupA = [];
    
    public ObservableCollection<WorkerDTO> WorkersGroupA
    {
        get => _workersGroupA;
        set {
            _workersGroupA = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<WorkerDTO> _workersGroupB = [];
    
    public ObservableCollection<WorkerDTO> WorkersGroupB
    {
        get => _workersGroupB;
        set {
            _workersGroupB = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<WorkerDTO> _workersGroupC = [];
    
    public ObservableCollection<WorkerDTO> WorkersGroupC
    {
        get => _workersGroupC;
        set {
            _workersGroupC = value;
            OnPropertyChanged();
        }
    }

    #endregion
    

    
    
    
    
    
    
    
    private long _skipFirstNReplicationsInPercent = 0;
    
    public long SkipFirstNReplicationsInPercent
    {
        get => _skipFirstNReplicationsInPercent;
        set
        {
            if (value == _skipFirstNReplicationsInPercent) return;
            _skipFirstNReplicationsInPercent = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RenderOffset));
        }
    }

    private long _renderPoints = 1000;
    
    public long RenderPoints
    {
        get => _renderPoints;
        set
        {
            _renderPoints = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RenderOffset));
        }
    }

    public long RenderOffset
    {
        get
        {
            var skipFirst = Math.Floor(Replications * (SkipFirstNReplicationsInPercent / 100.0));
            var renderOffset = (Replications - skipFirst) / RenderPoints;
            var result = Convert.ToInt64(Math.Floor(renderOffset)) - 1;
            return result >= 0 ? result : 0;
        }
    }
    
    private string _currentAverageWaitingTime = "? ";

    public string CurrentCosts
    {
        get => $"{_currentAverageWaitingTime}€";
        set
        {
            _currentAverageWaitingTime = value;
            OnPropertyChanged();
        }
    }
    
    private string _currentReplication = "?";

    public string CurrentReplication
    {
        get => _currentReplication;
        set
        {
            _currentReplication = value;
            OnPropertyChanged();
        }
    }
}
