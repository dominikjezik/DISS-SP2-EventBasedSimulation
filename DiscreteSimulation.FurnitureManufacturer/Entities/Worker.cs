using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class Worker
{
    public int Id { get; set; }
    
    public WorkerGroup Group { get; set; }

    public string DisplayId
    {
        get
        {
            if (Group == WorkerGroup.GroupA)
            {
                return $"A{Id}";
            }

            if (Group == WorkerGroup.GroupB)
            {
                return $"B{Id}";
            }
            
            if (Group == WorkerGroup.GroupC)
            {
                return $"C{Id}";
            }

            return Id.ToString();
        }
    }
    
    public bool IsBusy => CurrentOrder != null;
    
    // TODO: Pracovníkove vyťaženie štatistika
    
    public Order? CurrentOrder { get; set; }
    
    public bool IsInWarehouse { get; set; }
    
    public bool IsMovingToWarehouse { get; set; }
    
    public AssemblyLine? CurrentAssemblyLine { get; set; }
    
    public bool IsMovingToAssemblyLine { get; set; }
}
