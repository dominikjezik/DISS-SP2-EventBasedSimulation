namespace DiscreteSimulation.Core.FurnitureManufacturer.Entities;

public class AssemblyLine
{
    public int Id { get; set; }
    
    public Order? CurrentOrder { get; set; }
    
    public string Activity { get; set; }
    
    public Worker? CurrentWorker { get; set; }
}
