using DiscreteSimulation.FurnitureManufacturer.TicketStore.Entities;

namespace DiscreteSimulation.FurnitureManufacturer.TicketStore.Events;

public class ServiceFinished : TicketStoreBaseEvent
{
    public ServiceFinished(double time, TicketStoreSimulation eventSimulationCore, Customer customer) : base(time, eventSimulationCore)
    {
        CurrentCustomer = customer;
    }

    public override void Execute()
    {
        if (Simulation.CustomersQueue.Count == 0)
        {
            Simulation.IsStaffBusy = false;
        }
        else
        {
            var nextCustomer = Simulation.CustomersQueue.Dequeue();
            var serviceStarted = new ServiceStarted(Simulation.SimulationTime, Simulation, nextCustomer);
            Simulation.ScheduleEvent(serviceStarted);
        }
    }
}
