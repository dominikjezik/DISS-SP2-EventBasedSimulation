using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class ArrivalToLineWithCutMaterial : FurnitureManufacturerBaseEvent
{
    public AssemblyLine CurrentAssemblyLine { get; set; }
    
    public ArrivalToLineWithCutMaterial(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker, AssemblyLine assemblyLine) : base(time, eventSimulationCore, worker)
    {
        CurrentAssemblyLine = assemblyLine;
    }

    public override void Execute()
    {
        // Pracovník dorazil na linku, začína sa morenie a lakovanie materiálu
        CurrentWorker.IsMovingToAssemblyLine = false;
        CurrentWorker.CurrentAssemblyLine = CurrentAssemblyLine;
        CurrentAssemblyLine.CurrentWorker = CurrentWorker;
        CurrentWorker.CurrentOrder.State = "Varnishing";
        
        // Naplanovanie dokončenia morenia a lakovania materiálu
        double varnishingTime;
        
        switch (CurrentWorker.CurrentOrder.Type)
        {
            case FurnitureType.Desk:
                varnishingTime = Simulation.VarnishingDeskTimeGenerator.Next();
                break;
            case FurnitureType.Chair:
                varnishingTime = Simulation.VarnishingChairTimeGenerator.Next();
                break;
            case FurnitureType.Closet:
                varnishingTime = Simulation.VarnishingClosetTimeGenerator.Next();
                break;
            default:
                throw new ArgumentException("Unknown furniture type");
        }
        
        var materialVarnishingCompleted = new MaterialVarnishingCompleted(Simulation.SimulationTime + varnishingTime, Simulation, CurrentWorker);
        
        Simulation.ScheduleEvent(materialVarnishingCompleted);
    }
}