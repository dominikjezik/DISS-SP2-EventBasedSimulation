namespace DiscreteSimulation.Core.TicketStore.Entities;

public class Customer
{
    public double ArrivalTime { get; private set; }
    
    public Customer(double arrivalTime)
    {
        ArrivalTime = arrivalTime;
    }
}
