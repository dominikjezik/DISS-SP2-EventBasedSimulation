using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class MaterialPreparationCompleted : FurnitureManufacturerBaseEvent
{
    public MaterialPreparationCompleted(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker) : base(time, eventSimulationCore, worker)
    {
    }

    public override void Execute()
    {
        // Material bol pripravený, teraz ho pracovník prenáša na linku
        CurrentWorker.CurrentOrder.State = "Material prepared";
        CurrentWorker.IsInWarehouse = false;
        CurrentWorker.IsMovingToAssemblyLine = true;
            
        var arrivalTime = Simulation.SimulationTime + Simulation.ArrivalTimeBetweenLineAndWarehouseGenerator.Next();
        var materialDeliveredToLine = new MaterialDeliveredToLine(arrivalTime, Simulation, CurrentWorker);
        
        Simulation.ScheduleEvent(materialDeliveredToLine);
    }
}
