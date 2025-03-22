using DiscreteSimulation.Core.Generators;
using DiscreteSimulation.Core.SimulationCore;
using DiscreteSimulation.Core.TicketStore.Entities;
using DiscreteSimulation.Core.TicketStore.Events;

namespace DiscreteSimulation.Core.TicketStore;

public class TicketStoreSimulation : EventSimulationCore
{
    public ExponentialDistributionGenerator CustomerArrivalGenerator { get; private set; }
    
    public ExponentialDistributionGenerator ServicingTimeGenerator { get; private set; }
    
    public bool IsStaffBusy { get; set; } = false;
    
    public EntitiesQueue<Customer> CustomersQueue { get; private set; }
    
    public Statistics AverageWaitingTime { get; private set; } = new();
    
    public Statistics SimulationAverageWaitingTime { get; private set; } = new();
    
    public Statistics SimulationAverageQueueLength { get; private set; } = new();
    
    public TicketStoreSimulation()
    {
        CustomersQueue = new EntitiesQueue<Customer>(this);
    }

    public override void BeforeSimulation(int? seedForSeedGenerator = null)
    {
        base.BeforeSimulation(seedForSeedGenerator);
        
        CustomerArrivalGenerator = new ExponentialDistributionGenerator(1.0 / 8.0, SeedGenerator.Next());
        ServicingTimeGenerator = new ExponentialDistributionGenerator(1.0 / 7.0, SeedGenerator.Next());
        
        SimulationAverageWaitingTime.Clear();
        SimulationAverageQueueLength.Clear();
    }

    public override void BeforeReplication()
    {
        base.BeforeReplication();
        
        IsStaffBusy = false;
        CustomersQueue.Clear();
        AverageWaitingTime.Clear();
    }

    public override void ExecuteReplication()
    {
        // Naplanovanie prveho prichodu zakaznika
        var firstCustomerArrival = new CustomerArrival(0, this);
        ScheduleEvent(firstCustomerArrival);
        
        // Spustenie spracovavania udalosti
        base.ExecuteReplication();
    }
    
    public override void AfterReplication()
    {
        base.AfterReplication();
        
        // TODO: Ak replikacia nedobehla cela (simulacia bola predcasne ukoncena), tak sa nepridavaju hodnoty do statistik
        
        SimulationAverageWaitingTime.AddValue(AverageWaitingTime.Mean);
        SimulationAverageQueueLength.AddValue(CustomersQueue.AverageQueueLength);
    }
}
