using DiscreteSimulation.Core.Events;

namespace DiscreteSimulation.Core.SimulationCore;

public abstract class EventSimulationCore : MonteCarloSimulationCore
{
    private PriorityQueue<BaseEvent, double> _eventCalendar = new();
    
    public double SimulationTime { get; private set; } = 0;
    
    public double MaxReplicationTime { get; set; } = 0;

    public bool IsSimulationPaused { get; private set; } = false;
    
    public double SimulationSpeed { get; set; } = 1;
    
    public int SleepTime { get; set; } = 100;
    
    public double Delta => SimulationSpeed * SleepTime / 1000.0;
    
    protected bool IsReplicationStopped { get; set; } = false;
    
    public void ScheduleEvent(BaseEvent eventToSchedule)
    {
        if (eventToSchedule.Time < SimulationTime)
        {
            throw new InvalidOperationException("Event time is less than simulation time.");
        }
        
        _eventCalendar.Enqueue(eventToSchedule, eventToSchedule.Time);
    }

    public override void BeforeSimulation(int? seedForSeedGenerator = null)
    {
        base.BeforeSimulation(seedForSeedGenerator);
        IsReplicationStopped = false;
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
            var systemEvent = new SystemEvent(0, this);
            ScheduleEvent(systemEvent);
        }
        
        while (_eventCalendar.Count != 0 && SimulationTime <= MaxReplicationTime)
        {
            if (!IsSimulationRunning)
            {
                IsReplicationStopped = true;
                break;
            }
            
            while (IsSimulationPaused)
            {
                // Ak bola simulacia stopnuta počas toho ako bola pozastavená
                if (!IsSimulationRunning)
                {
                    IsReplicationStopped = true;
                    IsSimulationPaused = false;
                    break;
                }
                
                Thread.Sleep(200);
            }

            var nextEvent = _eventCalendar.Dequeue();
            
            if (nextEvent.Time < SimulationTime)
            {
                throw new InvalidOperationException("Event time is less than simulation time.");
            }
            
            if (nextEvent.Time > MaxReplicationTime)
            {
                break;
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
