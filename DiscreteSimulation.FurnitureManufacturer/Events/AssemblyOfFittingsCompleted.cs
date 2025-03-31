using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class AssemblyOfFittingsCompleted : FurnitureManufacturerBaseEvent
{
    public AssemblyOfFittingsCompleted(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker) : base(time, eventSimulationCore, worker)
    {
    }

    public override void Execute()
    {
        // Kovanie bolo namontované na skriňu, objednávka nábytku je ukončená
        var currentOrder = CurrentWorker.CurrentOrder;
        var currentAssemblyLine = CurrentWorker.CurrentAssemblyLine;
        
        CurrentWorker.CurrentOrder = null;

        Simulation.AverageProcessingOrderTime.AddValue(Simulation.SimulationTime - currentOrder.ArrivalTime);
        
        currentOrder.State = "Completed";
        
        Simulation.ReleaseAssemblyLine(currentAssemblyLine);
        
        // Pracovník zo skupiny C je voľný, preto najskôr prednostne skontrolujeme frontu čakajúcich zložených skríň
        if (Simulation.PendingFoldedClosetsQueue.Count > 0)
        {
            var pendingFoldedCloset = Simulation.PendingFoldedClosetsQueue.Dequeue();
            
            CurrentWorker.CurrentOrder = pendingFoldedCloset;
            CurrentWorker.IsMovingToAssemblyLine = true;
            
            Simulation.AverageWaitingTimeInPendingFoldedClosetsQueue.AddValue(Simulation.SimulationTime - pendingFoldedCloset.StartedWaitingTime);
            
            var arrivalTime = Simulation.SimulationTime + Simulation.ArrivalTimeBetweenTwoLinesGenerator.Next();
            var arrivalToLineWithFoldedCloset = new ArrivalToLineWithFoldedCloset(arrivalTime, Simulation, CurrentWorker, pendingFoldedCloset.CurrentAssemblyLine);
            
            Simulation.ScheduleEvent(arrivalToLineWithFoldedCloset);
        }
        // ak nie sú čakajúce zložené skrine, skontrolujeme front čakajúceho narezaného materiálu
        else if (Simulation.PendingCutMaterialsQueue.Count > 0)
        {
            var pendingCutMaterial = Simulation.PendingCutMaterialsQueue.Dequeue();
            
            CurrentWorker.CurrentOrder = pendingCutMaterial;
            CurrentWorker.IsMovingToAssemblyLine = true;
            
            Simulation.AverageWaitingTimeInPendingCutMaterialsQueue.AddValue(Simulation.SimulationTime - pendingCutMaterial.StartedWaitingTime);
            
            var arrivalTime = Simulation.SimulationTime + Simulation.ArrivalTimeBetweenTwoLinesGenerator.Next();
            var arrivalToLineWithCutMaterial = new ArrivalToLineWithCutMaterial(arrivalTime, Simulation, CurrentWorker, pendingCutMaterial.CurrentAssemblyLine);
            
            Simulation.ScheduleEvent(arrivalToLineWithCutMaterial);
        }
        else
        {
            currentAssemblyLine.IdleWorkers.Add(CurrentWorker);
        }
    }
}