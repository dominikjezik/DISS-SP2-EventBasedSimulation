namespace DiscreteSimulation.Core.FurnitureManufacturer.Entities;

public class Order
{
    public int Id { get; set; }
    
    public OrderType Type { get; set; }
    
    public string State { get; set; }
    
    public string Place { get; set; }
    
    public double ArrivalTime { get; set; }
}
