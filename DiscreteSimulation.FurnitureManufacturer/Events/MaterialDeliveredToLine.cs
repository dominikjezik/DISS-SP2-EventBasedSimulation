using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class MaterialDeliveredToLine : FurnitureManufacturerBaseEvent
{
    public MaterialDeliveredToLine(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker) : base(time, eventSimulationCore, worker)
    {
    }

    public override void Execute()
    {
        // Pracovník dorazil na linku, začína sa výroba - rezanie materiálu
        CurrentWorker.IsMovingToAssemblyLine = false;
        CurrentWorker.CurrentOrder.State = "Cutting material";

        // Vyhľadáme voľnú linku (od najmenšieho id), pričom
        // ak nie je žiadna voľná tak sa "vytvorí" nová linka
        var availableAssemblyLine = Simulation.RequestFreeAssemblyLine();
        
        availableAssemblyLine.CurrentOrder = CurrentWorker.CurrentOrder;
        CurrentWorker.CurrentOrder.CurrentAssemblyLine = availableAssemblyLine;
        availableAssemblyLine.CurrentWorker = CurrentWorker;
        CurrentWorker.CurrentAssemblyLine = availableAssemblyLine;
        
        // Naplanovanie dokončenia rezania materiálu
        double cuttingTime;
        
        switch (CurrentWorker.CurrentOrder.Type)
        {
            case FurnitureType.Desk:
                cuttingTime = Simulation.CuttingDeskTimeGenerator.Next();
                break;
            case FurnitureType.Chair:
                cuttingTime = Simulation.CuttingChairTimeGenerator.Next();
                break;
            case FurnitureType.Closet:
                cuttingTime = Simulation.CuttingClosetTimeGenerator.Next();
                break;
            default:
                throw new ArgumentException("Unknown furniture type");
        }
        
        var materialCuttingCompleted = new MaterialCuttingCompleted(Simulation.SimulationTime + cuttingTime, Simulation, CurrentWorker);
        
        Simulation.ScheduleEvent(materialCuttingCompleted);
    }
}
