using DiscreteSimulation.FurnitureManufacturer.TicketStore.Entities;

namespace DiscreteSimulation.FurnitureManufacturer.TicketStore.Events;

public class ServiceStarted : TicketStoreBaseEvent
{
    public ServiceStarted(double time, TicketStoreSimulation eventSimulationCore, Customer customer) : base(time, eventSimulationCore)
    {
        CurrentCustomer = customer;
    }

    public override void Execute()
    {
        var customerWaitingTime = Simulation.SimulationTime - CurrentCustomer.ArrivalTime;
        Simulation.AverageWaitingTime.AddValue(customerWaitingTime);
        
        Simulation.IsStaffBusy = true;
        
        // Naplanovanie ukoncenia obsluhy zakaznika
        var serviceFinished = new ServiceFinished(Simulation.SimulationTime + Simulation.ServicingTimeGenerator.Next(), Simulation, CurrentCustomer);
        Simulation.ScheduleEvent(serviceFinished);
    }
}
