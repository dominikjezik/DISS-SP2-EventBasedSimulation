using DiscreteSimulation.Core.SimulationCore;

namespace DiscreteSimulation.Core.Events;

public abstract class BaseEvent
{
    public double Time { get; protected set; }
    
    protected EventSimulationCore EventSimulationCore { get; private set; }
    
    protected BaseEvent(double time, EventSimulationCore eventSimulationCore)
    {
        Time = time;
        EventSimulationCore = eventSimulationCore;
    }
    
    public abstract void Execute();
}
