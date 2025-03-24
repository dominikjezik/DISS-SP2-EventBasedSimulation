using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class ArrivalToLineWithFoldedCloset : FurnitureManufacturerBaseEvent
{
    public AssemblyLine CurrentAssemblyLine { get; set; }
    
    public ArrivalToLineWithFoldedCloset(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker, AssemblyLine assemblyLine) : base(time, eventSimulationCore, worker)
    {
        CurrentAssemblyLine = assemblyLine;
    }

    public override void Execute()
    {
        // Pracovník dorazil na linku, začína sa montáž kovania skrine
        CurrentWorker.IsMovingToAssemblyLine = false;
        CurrentWorker.CurrentAssemblyLine = CurrentAssemblyLine;
        CurrentAssemblyLine.CurrentWorker = CurrentWorker;
        CurrentWorker.CurrentOrder.State = "Assembling fittings";
        
        // Naplanovanie dokončenia montáže kovania skrine
        var assemblyTime = Simulation.AssemblyOfFittingsOnClosetTimeGenerator.Next();
        var assemblyOfFittingsCompleted = new AssemblyOfFittingsCompleted(Simulation.SimulationTime + assemblyTime, Simulation, CurrentWorker);
        
        Simulation.ScheduleEvent(assemblyOfFittingsCompleted);
    }
}
