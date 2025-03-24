using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class ArrivalAtWarehouseForMaterial : FurnitureManufacturerBaseEvent
{
    public ArrivalAtWarehouseForMaterial(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker) : base(time, eventSimulationCore, worker)
    {
    }

    public override void Execute()
    {
        // Pracovník prišil do skladu, začína sa príprava materiálu
        CurrentWorker.IsMovingToWarehouse = false;
        CurrentWorker.IsInWarehouse = true;
        
        CurrentWorker.CurrentOrder.State = "Preparation in Warehouse";
        
        // Naplánovanie dokončenia prípravy materiálu
        var preparationCompletedTime = Simulation.SimulationTime + Simulation.MaterialPreparationTimeGenerator.Next();
        var materialPreparationCompleted = new MaterialPreparationCompleted(preparationCompletedTime, Simulation, CurrentWorker);
        
        Simulation.ScheduleEvent(materialPreparationCompleted);
    }
}
