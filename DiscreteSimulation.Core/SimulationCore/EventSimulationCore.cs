namespace DiscreteSimulation.Core.SimulationCore;

public abstract class EventSimulationCore : MonteCarloSimulationCore
{
    private PriorityQueue<BaseEvent, double> _eventCalendar = new();
    
    public double SimulationTime { get; private set; } = 0;
    
    public double MaxReplicationTime { get; set; } = 0;

    public bool IsSimulationPaused { get; private set; } = false;
    
    public double SimulationSpeed { get; set; } = 1;
    
    public void ScheduleEvent(BaseEvent eventToSchedule)
    {
        if (eventToSchedule.Time < SimulationTime)
        {
            throw new InvalidOperationException("Event time is less than simulation time.");
        }
        
        _eventCalendar.Enqueue(eventToSchedule, eventToSchedule.Time);
    }
    
    public override void BeforeReplication()
    {
        SimulationTime = 0;
        _eventCalendar.Clear();
    }
    
    public override void ExecuteReplication()
    {
        if (CurrentMaxReplications == 1)
        {
            // TODO: Parametre delta a sleep ms vytiahnut na GUI
            
            var systemEvent = new SystemEvent(0, this);
            ScheduleEvent(systemEvent);
        }
        
        while (_eventCalendar.Count != 0 && IsSimulationRunning && SimulationTime < MaxReplicationTime)
        {
            while (IsSimulationPaused)
            {
                Thread.Sleep(200);
            }

            var nextEvent = _eventCalendar.Dequeue();
            
            if (nextEvent.Time < SimulationTime)
            {
                throw new InvalidOperationException("Event time is less than simulation time.");
            }
            
            SimulationTime = nextEvent.Time;
            nextEvent.Execute();
            
            if (CurrentMaxReplications == 1)
            {
                SimulationStateChanged?.Invoke();
            }
        }
    }
    
    public event Action? SimulationStateChanged;
    
    public void PauseSimulation()
    {
        IsSimulationPaused = true;
    }
    
    public void ResumeSimulation()
    {
        IsSimulationPaused = false;
    }
}
