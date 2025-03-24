namespace DiscreteSimulation.FurnitureManufacturer.TicketStore.Entities;

public class Customer
{
    public double ArrivalTime { get; private set; }
    
    public Customer(double arrivalTime)
    {
        ArrivalTime = arrivalTime;
    }
}
