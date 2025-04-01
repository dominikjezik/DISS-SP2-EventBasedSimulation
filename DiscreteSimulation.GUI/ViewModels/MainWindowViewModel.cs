using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DiscreteSimulation.FurnitureManufacturer.DTOs;
using DiscreteSimulation.FurnitureManufacturer.Simulation;

namespace DiscreteSimulation.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FurnitureManufacturerSimulation _simulation = new();
    
    public FurnitureManufacturerSimulation Simulation => _simulation;
    
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
            
            OnPropertyChanged(nameof(Delta));
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
        PauseResumeSimulationButtonText = "Pause";
    }
    
    private int _countOfWorkersGroupA = 2;
    
    public int CountOfWorkersGroupA
    {
        get => _countOfWorkersGroupA;
        set
        {
            _countOfWorkersGroupA = value;
            OnPropertyChanged();
        }
    }
    
    private int _countOfWorkersGroupB = 2;
    
    public int CountOfWorkersGroupB
    {
        get => _countOfWorkersGroupB;
        set
        {
            _countOfWorkersGroupB = value;
            OnPropertyChanged();
        }
    }
    
    private int _countOfWorkersGroupC = 18;
    
    public int CountOfWorkersGroupC
    {
        get => _countOfWorkersGroupC;
        set
        {
            _countOfWorkersGroupC = value;
            OnPropertyChanged();
        }
    }
    
    private bool _enableWorkerLocationPreference = true;
    
    public bool EnableWorkerLocationPreference
    {
        get => _enableWorkerLocationPreference;
        set
        {
            _enableWorkerLocationPreference = value;
            OnPropertyChanged();
        }
    }
    
    #endregion

    #region ReplicationControls

    private long _replications = 1000;
    
    public long Replications
    {
        get => _replications;
        set
        {
            if (value == _replications) return;
            _replications = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RenderOffset));
            OnPropertyChanged(nameof(IsSingleReplication));
            OnPropertyChanged(nameof(IsMultipleReplications));

            if (value < RenderPoints)
            {
                RenderPoints = value;
            }
        }
    }
    
    public bool IsSingleReplication => Replications == 1;
    
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
    
    private string _selectedTimeUnits = "seconds";
    
    public string SelectedTimeUnits
    {
        get => _selectedTimeUnits;
        set
        {
            _selectedTimeUnits = value;
            OnPropertyChanged();
            
            OnPropertyChanged(nameof(ReplicationOrderProcessingTime));
            OnPropertyChanged(nameof(ReplicationPendingOrdersWaitingTime));
            OnPropertyChanged(nameof(ReplicationCutMaterialsWaitingTime));
            OnPropertyChanged(nameof(ReplicationVarnishedMaterialsWaitingTime));
            OnPropertyChanged(nameof(ReplicationFoldedClosetsWaitingTime));
            
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTime));
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTimeCI));
            OnPropertyChanged(nameof(SimulationPendingOrdersWaitingTime));
            OnPropertyChanged(nameof(SimulationPendingOrdersWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationCutMaterialsWaitingTime));
            OnPropertyChanged(nameof(SimulationCutMaterialsWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationVarnishedMaterialsWaitingTime));
            OnPropertyChanged(nameof(SimulationVarnishedMaterialsWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationFoldedClosetsWaitingTime));
            OnPropertyChanged(nameof(SimulationFoldedClosetsWaitingTimeCI));
        }
    }
    
    #endregion
    
    #region SingleReplicationControls
    
    public int SleepTime
    {
        get => _simulation.SleepTime;
        set
        {
            _simulation.SleepTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Delta));
        }
    }

    public double Delta => _simulation.Delta;
    
    private string _currentSimulationTime = "[Week 1 - Monday] 06:00:00";
    
    public string CurrentSimulationTime
    {
        get => _currentSimulationTime;
        set
        {
            _currentSimulationTime = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationOrderProcessingTimeSeconds = "-";
    private string _replicationOrderProcessingTimeMinutes = "-";
    private string _replicationOrderProcessingTimeHours = "-";
    
    public string ReplicationOrderProcessingTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _replicationOrderProcessingTimeSeconds,
                "minutes" => _replicationOrderProcessingTimeMinutes,
                "hours" => _replicationOrderProcessingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationOrderProcessingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationOrderProcessingTimeSeconds = $"{averageTime:F2}";
        _replicationOrderProcessingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationOrderProcessingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationOrderProcessingTime));
    }
    
    private string _replicationPendingOrders = "-";
    
    public string ReplicationPendingOrders
    {
        get => _replicationPendingOrders;
        set
        {
            _replicationPendingOrders = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingCutMaterials = "-";
    
    public string ReplicationPendingCutMaterials
    {
        get => _replicationPendingCutMaterials;
        set
        {
            _replicationPendingCutMaterials = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingVarnishedMaterials = "-";
    
    public string ReplicationPendingVarnishedMaterials
    {
        get => _replicationPendingVarnishedMaterials;
        set
        {
            _replicationPendingVarnishedMaterials = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingFoldedClosets = "-";
    
    public string ReplicationPendingFoldedClosets
    {
        get => _replicationPendingFoldedClosets;
        set
        {
            _replicationPendingFoldedClosets = value;
            OnPropertyChanged();
        }
    }
    
    
    private string _replicationPendingOrdersWaitingTimeSeconds = "-";
    private string _replicationPendingOrdersWaitingTimeMinutes = "-";
    private string _replicationPendingOrdersWaitingTimeHours = "-";
    
    public string ReplicationPendingOrdersWaitingTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _replicationPendingOrdersWaitingTimeSeconds,
                "minutes" => _replicationPendingOrdersWaitingTimeMinutes,
                "hours" => _replicationPendingOrdersWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingOrdersWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingOrdersWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingOrdersWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingOrdersWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingOrdersWaitingTime));
    }
    
    private string _replicationCutMaterialsWaitingTimeSeconds = "-";
    private string _replicationCutMaterialsWaitingTimeMinutes = "-";
    private string _replicationCutMaterialsWaitingTimeHours = "-";
    
    public string ReplicationCutMaterialsWaitingTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _replicationCutMaterialsWaitingTimeSeconds,
                "minutes" => _replicationCutMaterialsWaitingTimeMinutes,
                "hours" => _replicationCutMaterialsWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationCutMaterialsWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationCutMaterialsWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationCutMaterialsWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationCutMaterialsWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationCutMaterialsWaitingTime));
    }
    
    private string _replicationVarnishedMaterialsWaitingTimeSeconds = "-";
    private string _replicationVarnishedMaterialsWaitingTimeMinutes = "-";
    private string _replicationVarnishedMaterialsWaitingTimeHours = "-";
    
    public string ReplicationVarnishedMaterialsWaitingTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _replicationVarnishedMaterialsWaitingTimeSeconds,
                "minutes" => _replicationVarnishedMaterialsWaitingTimeMinutes,
                "hours" => _replicationVarnishedMaterialsWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationVarnishedMaterialsWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationVarnishedMaterialsWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationVarnishedMaterialsWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationVarnishedMaterialsWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationVarnishedMaterialsWaitingTime));
    }
    
    private string _replicationFoldedClosetsWaitingTimeSeconds = "-";
    private string _replicationFoldedClosetsWaitingTimeMinutes = "-";
    private string _replicationFoldedClosetsWaitingTimeHours = "-";
    
    public string ReplicationFoldedClosetsWaitingTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _replicationFoldedClosetsWaitingTimeSeconds,
                "minutes" => _replicationFoldedClosetsWaitingTimeMinutes,
                "hours" => _replicationFoldedClosetsWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationFoldedClosetsWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationFoldedClosetsWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationFoldedClosetsWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationFoldedClosetsWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationFoldedClosetsWaitingTime));
    }
    
    
    
    private string _pendingOrdersQueueCount = "0";

    public string PendingOrdersQueueCount
    {
        get => _pendingOrdersQueueCount;
        set
        {
            _pendingOrdersQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingOrdersQueueTabTitle));
        }
    }
    
    public string PendingOrdersQueueTabTitle => $"To Process ({PendingOrdersQueueCount})";
    
    private string _pendingCutMaterialsQueueCount = "0";
    
    public string PendingCutMaterialsQueueCount
    {
        get => _pendingCutMaterialsQueueCount;
        set
        {
            _pendingCutMaterialsQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingCutMaterialsQueueTabTitle));
        }
    }
    public string PendingCutMaterialsQueueTabTitle => $"Cut Materials ({PendingCutMaterialsQueueCount})";

    
    private string _pendingVarnishedMaterialsQueueCount = "0";
    
    public string PendingVarnishedMaterialsQueueCount
    {
        get => _pendingVarnishedMaterialsQueueCount;
        set
        {
            _pendingVarnishedMaterialsQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingVarnishedMaterialsQueueTabTitle));
        }
    }
    
    public string PendingVarnishedMaterialsQueueTabTitle => $"Varnished Materials ({PendingVarnishedMaterialsQueueCount})";

    
    private string _pendingFoldedClosetsQueueCount = "0";
    
    public string PendingFoldedClosetsQueueCount
    {
        get => _pendingFoldedClosetsQueueCount;
        set
        {
            _pendingFoldedClosetsQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingFoldedClosetsQueueTabTitle));
        }
    }
    
    public string PendingFoldedClosetsQueueTabTitle => $"Folded Closets ({PendingFoldedClosetsQueueCount})";


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
    
    private ObservableCollection<OrderDTO> _pendingOrdersQueue = [];

    public ObservableCollection<OrderDTO> PendingOrdersQueue
    {
        get => _pendingOrdersQueue;
        set {
            _pendingOrdersQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<OrderDTO> _pendingCutMaterialsQueue = [];

    public ObservableCollection<OrderDTO> PendingCutMaterialsQueue
    {
        get => _pendingCutMaterialsQueue;
        set {
            _pendingCutMaterialsQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<OrderDTO> _pendingVarnishedMaterialsQueue = [];

    public ObservableCollection<OrderDTO> PendingVarnishedMaterialsQueue
    {
        get => _pendingVarnishedMaterialsQueue;
        set {
            _pendingVarnishedMaterialsQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<OrderDTO> _pendingFoldedClosetsQueue = [];

    public ObservableCollection<OrderDTO> PendingFoldedClosetsQueue
    {
        get => _pendingFoldedClosetsQueue;
        set {
            _pendingFoldedClosetsQueue = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationWorkersGroupAUtilization = "-";
    
    public string ReplicationWorkersGroupAUtilization
    {
        get => _replicationWorkersGroupAUtilization;
        set
        {
            _replicationWorkersGroupAUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationWorkersGroupBUtilization = "-";
    
    public string ReplicationWorkersGroupBUtilization
    {
        get => _replicationWorkersGroupBUtilization;
        set
        {
            _replicationWorkersGroupBUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationWorkersGroupCUtilization = "-";
    
    public string ReplicationWorkersGroupCUtilization
    {
        get => _replicationWorkersGroupCUtilization;
        set
        {
            _replicationWorkersGroupCUtilization = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region MultipleReplicationsControls
    
    private bool _enableRender95ConfidenceInterval = true;
    
    public bool EnableRender95ConfidenceInterval
    {
        get => _enableRender95ConfidenceInterval;
        set
        {
            _enableRender95ConfidenceInterval = value;
            OnPropertyChanged();
        }
    }
    
    private string _currentReplication = "_";

    public string CurrentReplication
    {
        get => _currentReplication;
        set
        {
            _currentReplication = value;
            OnPropertyChanged();
        }
    }

    private string _simulationCurrentProcessingOrderTimeSeconds = "-";
    private string _simulationCurrentProcessingOrderTimeMinutes = "-";
    private string _simulationCurrentProcessingOrderTimeHours = "-";
    
    public string SimulationCurrentProcessingOrderTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _simulationCurrentProcessingOrderTimeSeconds,
                "minutes" => _simulationCurrentProcessingOrderTimeMinutes,
                "hours" => _simulationCurrentProcessingOrderTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationCurrentProcessingOrderTimeSecondsCI = "-";
    private string _simulationCurrentProcessingOrderTimeMinutesCI = "-";
    private string _simulationCurrentProcessingOrderTimeHoursCI = "-";
    
    public string SimulationCurrentProcessingOrderTimeCI
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _simulationCurrentProcessingOrderTimeSecondsCI,
                "minutes" => _simulationCurrentProcessingOrderTimeMinutesCI,
                "hours" => _simulationCurrentProcessingOrderTimeHoursCI,
                _ => "-"
            };
        }
    }

    public void SetSimulationCurrentProcessingOrderTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationCurrentProcessingOrderTimeSeconds = $"{averageTime:F2}";
        _simulationCurrentProcessingOrderTimeMinutes = $"{averageTime / 60:F2}";
        _simulationCurrentProcessingOrderTimeHours = $"{averageTime / 3600:F2}";
        
        _simulationCurrentProcessingOrderTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationCurrentProcessingOrderTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationCurrentProcessingOrderTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTime));
        OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTimeCI));
    }
    
    private string _simulationPendingOrders = "-";
    
    public string SimulationPendingOrders
    {
        get => _simulationPendingOrders;
        set
        {
            _simulationPendingOrders = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingOrdersCI = "-";
    
    public string SimulationPendingOrdersCI
    {
        get => _simulationPendingOrdersCI;
        set
        {
            _simulationPendingOrdersCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingCutMaterials = "-";
    
    public string SimulationPendingCutMaterials
    {
        get => _simulationPendingCutMaterials;
        set
        {
            _simulationPendingCutMaterials = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingCutMaterialsCI = "-";
    
    public string SimulationPendingCutMaterialsCI
    {
        get => _simulationPendingCutMaterialsCI;
        set
        {
            _simulationPendingCutMaterialsCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingVarnishedMaterials = "-";
    
    public string SimulationPendingVarnishedMaterials
    {
        get => _simulationPendingVarnishedMaterials;
        set
        {
            _simulationPendingVarnishedMaterials = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingVarnishedMaterialsCI = "-";
    
    public string SimulationPendingVarnishedMaterialsCI
    {
        get => _simulationPendingVarnishedMaterialsCI;
        set
        {
            _simulationPendingVarnishedMaterialsCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingFoldedClosets = "-";
    
    public string SimulationPendingFoldedClosets
    {
        get => _simulationPendingFoldedClosets;
        set
        {
            _simulationPendingFoldedClosets = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingFoldedClosetsCI = "-";
    
    public string SimulationPendingFoldedClosetsCI
    {
        get => _simulationPendingFoldedClosetsCI;
        set
        {
            _simulationPendingFoldedClosetsCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingOrdersWaitingTimeSeconds = "-";
    private string _simulationPendingOrdersWaitingTimeMinutes = "-";
    private string _simulationPendingOrdersWaitingTimeHours = "-";
    
    public string SimulationPendingOrdersWaitingTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _simulationPendingOrdersWaitingTimeSeconds,
                "minutes" => _simulationPendingOrdersWaitingTimeMinutes,
                "hours" => _simulationPendingOrdersWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationPendingOrdersWaitingTimeSecondsCI = "-";
    private string _simulationPendingOrdersWaitingTimeMinutesCI = "-";
    private string _simulationPendingOrdersWaitingTimeHoursCI = "-";
    
    public string SimulationPendingOrdersWaitingTimeCI
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _simulationPendingOrdersWaitingTimeSecondsCI,
                "minutes" => _simulationPendingOrdersWaitingTimeMinutesCI,
                "hours" => _simulationPendingOrdersWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }
    
    public void SetSimulationPendingOrdersWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationPendingOrdersWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationPendingOrdersWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationPendingOrdersWaitingTimeHours = $"{averageTime / 3600:F2}";
           
        _simulationPendingOrdersWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationPendingOrdersWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationPendingOrdersWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationPendingOrdersWaitingTime));
        OnPropertyChanged(nameof(SimulationPendingOrdersWaitingTimeCI));
    }
    
    
    private string _simulationCutMaterialsWaitingTimeSeconds = "-";
    private string _simulationCutMaterialsWaitingTimeMinutes = "-";
    private string _simulationCutMaterialsWaitingTimeHours = "-";
    
    public string SimulationCutMaterialsWaitingTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _simulationCutMaterialsWaitingTimeSeconds,
                "minutes" => _simulationCutMaterialsWaitingTimeMinutes,
                "hours" => _simulationCutMaterialsWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationCutMaterialsWaitingTimeSecondsCI = "-";
    private string _simulationCutMaterialsWaitingTimeMinutesCI = "-";
    private string _simulationCutMaterialsWaitingTimeHoursCI = "-";
    
    public string SimulationCutMaterialsWaitingTimeCI
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _simulationCutMaterialsWaitingTimeSecondsCI,
                "minutes" => _simulationCutMaterialsWaitingTimeMinutesCI,
                "hours" => _simulationCutMaterialsWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }
    
    public void SetSimulationCutMaterialsWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationCutMaterialsWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationCutMaterialsWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationCutMaterialsWaitingTimeHours = $"{averageTime / 3600:F2}";
           
        _simulationCutMaterialsWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationCutMaterialsWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationCutMaterialsWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationCutMaterialsWaitingTime));
        OnPropertyChanged(nameof(SimulationCutMaterialsWaitingTimeCI));
    }
    
    
    private string _simulationVarnishedMaterialsWaitingTimeSeconds = "-";
    private string _simulationVarnishedMaterialsWaitingTimeMinutes = "-";
    private string _simulationVarnishedMaterialsWaitingTimeHours = "-";

    public string SimulationVarnishedMaterialsWaitingTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _simulationVarnishedMaterialsWaitingTimeSeconds,
                "minutes" => _simulationVarnishedMaterialsWaitingTimeMinutes,
                "hours" => _simulationVarnishedMaterialsWaitingTimeHours,
                _ => "-"
            };
        }
    }

    private string _simulationVarnishedMaterialsWaitingTimeSecondsCI = "-";
    private string _simulationVarnishedMaterialsWaitingTimeMinutesCI = "-";
    private string _simulationVarnishedMaterialsWaitingTimeHoursCI = "-";

    public string SimulationVarnishedMaterialsWaitingTimeCI
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _simulationVarnishedMaterialsWaitingTimeSecondsCI,
                "minutes" => _simulationVarnishedMaterialsWaitingTimeMinutesCI,
                "hours" => _simulationVarnishedMaterialsWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }

    public void SetSimulationVarnishedMaterialsWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationVarnishedMaterialsWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationVarnishedMaterialsWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationVarnishedMaterialsWaitingTimeHours = $"{averageTime / 3600:F2}";
            
        _simulationVarnishedMaterialsWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationVarnishedMaterialsWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationVarnishedMaterialsWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationVarnishedMaterialsWaitingTime));
        OnPropertyChanged(nameof(SimulationVarnishedMaterialsWaitingTimeCI));
    }
    
    
    private string _simulationFoldedClosetsWaitingTimeSeconds = "-";
    private string _simulationFoldedClosetsWaitingTimeMinutes = "-";
    private string _simulationFoldedClosetsWaitingTimeHours = "-";

    public string SimulationFoldedClosetsWaitingTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _simulationFoldedClosetsWaitingTimeSeconds,
                "minutes" => _simulationFoldedClosetsWaitingTimeMinutes,
                "hours" => _simulationFoldedClosetsWaitingTimeHours,
                _ => "-"
            };
        }
    }

    private string _simulationFoldedClosetsWaitingTimeSecondsCI = "-";
    private string _simulationFoldedClosetsWaitingTimeMinutesCI = "-";
    private string _simulationFoldedClosetsWaitingTimeHoursCI = "-";

    public string SimulationFoldedClosetsWaitingTimeCI
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => _simulationFoldedClosetsWaitingTimeSecondsCI,
                "minutes" => _simulationFoldedClosetsWaitingTimeMinutesCI,
                "hours" => _simulationFoldedClosetsWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }

    public void SetSimulationFoldedClosetsWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationFoldedClosetsWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationFoldedClosetsWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationFoldedClosetsWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        _simulationFoldedClosetsWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationFoldedClosetsWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationFoldedClosetsWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationFoldedClosetsWaitingTime));
        OnPropertyChanged(nameof(SimulationFoldedClosetsWaitingTimeCI));
    }
    
    
    private string _simulationWorkersAUtilization = "-";
    
    public string SimulationWorkersAUtilization
    {
        get => _simulationWorkersAUtilization;
        set
        {
            _simulationWorkersAUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationWorkersAUtilizationCI = "-";
    
    public string SimulationWorkersAUtilizationCI
    {
        get => _simulationWorkersAUtilizationCI;
        set
        {
            _simulationWorkersAUtilizationCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationWorkersBUtilization = "-";
    
    public string SimulationWorkersBUtilization
    {
        get => _simulationWorkersBUtilization;
        set
        {
            _simulationWorkersBUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationWorkersBUtilizationCI = "-";
    
    public string SimulationWorkersBUtilizationCI
    {
        get => _simulationWorkersBUtilizationCI;
        set
        {
            _simulationWorkersBUtilizationCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationWorkersCUtilization = "-";
    
    public string SimulationWorkersCUtilization
    {
        get => _simulationWorkersBUtilization;
        set
        {
            _simulationWorkersBUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationWorkersCUtilizationCI = "-";
    
    public string SimulationWorkersCUtilizationCI
    {
        get => _simulationWorkersCUtilizationCI;
        set
        {
            _simulationWorkersCUtilizationCI = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<WorkerDTO> _simulationAllWorkersUtilization = [];
    
    public ObservableCollection<WorkerDTO> SimulationAllWorkersUtilization
    {
        get => _simulationAllWorkersUtilization;
        set {
            _simulationAllWorkersUtilization = value;
            OnPropertyChanged();
        }
    }
    
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

    #endregion
    
}
