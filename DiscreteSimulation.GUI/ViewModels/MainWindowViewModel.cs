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
    
    #endregion

    #region ReplicationControls

    private long _replications = 10;
    
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
    
    private string _replicationOrderProcessingTime = "-";

    public string ReplicationOrderProcessingTime
    {
        get => _replicationOrderProcessingTime;
        set
        {
            _replicationOrderProcessingTime = value;
            OnPropertyChanged();
        }
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
    
    private string _selectedTimeUnits = "seconds";
    
    public string SelectedTimeUnits
    {
        get => _selectedTimeUnits;
        set
        {
            _selectedTimeUnits = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTime));
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTimeCI));
        }
    }

    public string SimulationCurrentProcessingOrderTime
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => SimulationCurrentProcessingOrderTimeSeconds,
                "minutes" => SimulationCurrentProcessingOrderTimeMinutes,
                "hours" => SimulationCurrentProcessingOrderTimeHours,
                _ => "-"
            };
        }
    }

    private string _simulationCurrentProcessingOrderTimeSeconds = "-";

    public string SimulationCurrentProcessingOrderTimeSeconds
    {
        get => _simulationCurrentProcessingOrderTimeSeconds;
        set
        {
            _simulationCurrentProcessingOrderTimeSeconds = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTime));
        }
    }
    
    private string _simulationCurrentProcessingOrderTimeMinutes = "-";

    public string SimulationCurrentProcessingOrderTimeMinutes
    {
        get => _simulationCurrentProcessingOrderTimeMinutes;
        set
        {
            _simulationCurrentProcessingOrderTimeMinutes = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTime));
        }
    }
    
    private string _simulationCurrentProcessingOrderTimeHours = "-";

    public string SimulationCurrentProcessingOrderTimeHours
    {
        get => _simulationCurrentProcessingOrderTimeHours;
        set
        {
            _simulationCurrentProcessingOrderTimeHours = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTime));
        }
    }
    
    public string SimulationCurrentProcessingOrderTimeCI
    {
        get
        {
            return _selectedTimeUnits switch
            {
                "seconds" => SimulationCurrentProcessingOrderTimeSecondsCI,
                "minutes" => SimulationCurrentProcessingOrderTimeMinutesCI,
                "hours" => SimulationCurrentProcessingOrderTimeHoursCI,
                _ => "-"
            };
        }
    }
    
    private string _simulationCurrentProcessingOrderTimeSecondsCI = "-";
    
    public string SimulationCurrentProcessingOrderTimeSecondsCI
    {
        get => _simulationCurrentProcessingOrderTimeSecondsCI;
        set
        {
            _simulationCurrentProcessingOrderTimeSecondsCI = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTimeCI));
        }
    }
    
    private string _simulationCurrentProcessingOrderTimeMinutesCI = "-";
    
    public string SimulationCurrentProcessingOrderTimeMinutesCI
    {
        get => _simulationCurrentProcessingOrderTimeMinutesCI;
        set
        {
            _simulationCurrentProcessingOrderTimeMinutesCI = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTimeCI));
        }
    }
    
    private string _simulationCurrentProcessingOrderTimeHoursCI = "-";
    
    public string SimulationCurrentProcessingOrderTimeHoursCI
    {
        get => _simulationCurrentProcessingOrderTimeHoursCI;
        set
        {
            _simulationCurrentProcessingOrderTimeHoursCI = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTimeCI));
        }
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

    private long _renderPoints = 10;

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
