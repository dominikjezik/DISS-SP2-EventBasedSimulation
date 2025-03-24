using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class ArrivalToLineWithVarnishedMaterial : FurnitureManufacturerBaseEvent
{
    public AssemblyLine CurrentAssemblyLine { get; set; }
    
    public ArrivalToLineWithVarnishedMaterial(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker, AssemblyLine assemblyLine) : base(time, eventSimulationCore, worker)
    {
        CurrentAssemblyLine = assemblyLine;
    }

    public override void Execute()
    {
        // Pracovník dorazil na linku, začína sa skladanie nábytku
        CurrentWorker.IsMovingToAssemblyLine = false;
        CurrentWorker.CurrentAssemblyLine = CurrentAssemblyLine;
        CurrentAssemblyLine.CurrentWorker = CurrentWorker;
        CurrentWorker.CurrentOrder.State = "Folding";
        
        // Naplanovanie dokončenia skladania nábytku
        double foldingTime;
        
        switch (CurrentWorker.CurrentOrder.Type)
        {
            case FurnitureType.Desk:
                foldingTime = Simulation.FoldingDeskTimeGenerator.Next();
                break;
            case FurnitureType.Chair:
                foldingTime = Simulation.FoldingChairTimeGenerator.Next();
                break;
            case FurnitureType.Closet:
                foldingTime = Simulation.FoldingClosetTimeGenerator.Next();
                break;
            default:
                throw new ArgumentException("Unknown furniture type");
        }
        
        var furnitureFoldingCompleted = new FurnitureFoldingCompleted(Simulation.SimulationTime + foldingTime, Simulation, CurrentWorker);
        
        Simulation.ScheduleEvent(furnitureFoldingCompleted);
    }
}
