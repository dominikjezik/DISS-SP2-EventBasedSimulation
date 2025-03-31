using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class Order
{
    public int Id { get; set; }
    
    public FurnitureType Type { get; set; }
    
    public AssemblyLine? CurrentAssemblyLine { get; set; }
    
    public double ArrivalTime { get; set; }
    
    public double StartedWaitingTime { get; set; }
    
    public string State { get; set; }
}
