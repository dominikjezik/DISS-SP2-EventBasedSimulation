using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class StartOfOrderPreparation : FurnitureManufacturerBaseEvent
{
    public StartOfOrderPreparation(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker) : base(time, eventSimulationCore, worker)
    {
    }

    public override void Execute()
    {
        if (CurrentWorker.IsInWarehouse)
        {
            // Pracovník je v sklade, začína sa príprava materiálu
            CurrentWorker.CurrentOrder.State = "Preparation in Warehouse";
            
            // Naplánovanie dokončenia prípravy materiálu
            var preparationCompletedTime = Simulation.SimulationTime + Simulation.MaterialPreparationTimeGenerator.Next();
            var materialPreparationCompleted = new MaterialPreparationCompleted(preparationCompletedTime, Simulation, CurrentWorker);
            
            Simulation.ScheduleEvent(materialPreparationCompleted);
        }
        else
        {
            // Pracovník nie je v sklade preto sa tam musí najskôr dostať
            CurrentWorker.CurrentOrder.State = "Order assigned";
            CurrentWorker.CurrentAssemblyLine?.IdleWorkers.Remove(CurrentWorker);
            CurrentWorker.IsMovingToWarehouse = true;
            
            var arrivalTime = Simulation.SimulationTime + Simulation.ArrivalTimeBetweenLineAndWarehouseGenerator.Next();
            var arrivalAtWarehouseForMaterial = new ArrivalAtWarehouseForMaterial(arrivalTime, Simulation, CurrentWorker);
            
            Simulation.ScheduleEvent(arrivalAtWarehouseForMaterial);
        }
        
        
        
    }
}
