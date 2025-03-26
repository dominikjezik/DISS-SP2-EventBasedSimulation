using DiscreteSimulation.Core.SimulationCore;

namespace DiscreteSimulation.Core.Events;

internal class SystemEvent : BaseEvent
{
    public SystemEvent(double time, EventSimulationCore eventSimulationCore) : base(time, eventSimulationCore)
    {
    }

    public override void Execute()
    {
        if (double.IsInfinity(EventSimulationCore.SimulationSpeed))
        {
            return;
        }
        
        Thread.Sleep(EventSimulationCore.SleepTime);
        
        Time = EventSimulationCore.SimulationTime + EventSimulationCore.Delta;
        EventSimulationCore.ScheduleEvent(this);
    }
}
