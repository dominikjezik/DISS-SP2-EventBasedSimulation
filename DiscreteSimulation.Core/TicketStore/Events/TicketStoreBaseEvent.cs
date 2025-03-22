using DiscreteSimulation.Core.SimulationCore;
using DiscreteSimulation.Core.TicketStore.Entities;

namespace DiscreteSimulation.Core.TicketStore.Events;

public abstract class TicketStoreBaseEvent : BaseEvent
{
    protected Customer CurrentCustomer { get; set; }
    
    protected TicketStoreSimulation Simulation { get; private set; }
    
    protected TicketStoreBaseEvent(double time, TicketStoreSimulation eventSimulationCore) : base(time, eventSimulationCore)
    {
        Simulation = eventSimulationCore;
    }
}
