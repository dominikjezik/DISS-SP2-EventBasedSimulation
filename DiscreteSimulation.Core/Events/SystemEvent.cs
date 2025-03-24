using DiscreteSimulation.Core.SimulationCore;

namespace DiscreteSimulation.Core.Events;

internal class SystemEvent : BaseEvent
{
    public SystemEvent(double time, EventSimulationCore eventSimulationCore) : base(time, eventSimulationCore)
    {
    }

    public override void Execute()
    {
        // TODO: Parametre delta a sleep ms vytiahnut na GUI
        
        // var delta = 1;
        // var simulationSpeed = EventSimulationCore.SimulationSpeed;
        // var sleepTime = (delta / simulationSpeed) * 1000;
        
        if (double.IsInfinity(EventSimulationCore.SimulationSpeed))
        {
            return;
        }

        var sleepTime = 100;
        var simulationSpeed = EventSimulationCore.SimulationSpeed;
        var delta = simulationSpeed * sleepTime / 1000;
        
        Thread.Sleep(sleepTime);
        
        Time = EventSimulationCore.SimulationTime + delta;
        EventSimulationCore.ScheduleEvent(this);
    }
}
