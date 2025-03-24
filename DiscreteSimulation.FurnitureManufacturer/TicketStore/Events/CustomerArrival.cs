using DiscreteSimulation.FurnitureManufacturer.TicketStore.Entities;

namespace DiscreteSimulation.FurnitureManufacturer.TicketStore.Events;

public class CustomerArrival : TicketStoreBaseEvent
{
    public CustomerArrival(double time, TicketStoreSimulation eventSimulationCore) : base(time, eventSimulationCore)
    {
    }

    public override void Execute()
    {
        CurrentCustomer = new Customer(Simulation.SimulationTime);
        
        if (Simulation.IsStaffBusy)
        {
            Simulation.CustomersQueue.Enqueue(CurrentCustomer);
        }
        else
        {
            var serviceStarted = new ServiceStarted(Simulation.SimulationTime, Simulation, CurrentCustomer);
            Simulation.ScheduleEvent(serviceStarted);
        }
        
        // Naplanovanie dalsieho prichodu zakaznika
        Time = Simulation.SimulationTime + Simulation.CustomerArrivalGenerator.Next();
        CurrentCustomer = null;
        Simulation.ScheduleEvent(this);
    }
}
