using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class MaterialVarnishingCompleted : FurnitureManufacturerBaseEvent
{
    public MaterialVarnishingCompleted(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker) : base(time, eventSimulationCore, worker)
    {
    }

    public override void Execute()
    {
        // Materiál bol namorený a nalakovaný
        // Ak je k dispozícií pracovník zo skupiny B
        // tak sa presunie k linke s týmto materiálom
        
        var currentOrder = CurrentWorker.CurrentOrder;
        var currentAssemblyLine = CurrentWorker.CurrentAssemblyLine;
        
        CurrentWorker.CurrentOrder = null;

        var availableWorker = Simulation.GetAvailableWorker(WorkerGroup.GroupB, currentAssemblyLine);
        
        currentAssemblyLine.CurrentWorker = availableWorker;

        if (availableWorker == null)
        {
            // Pracovník nie je k dispozícii, materiál sa pridá do frontu čakajúcich namorených a nalakovaných materiálov
            currentOrder.State = "Varnished (waiting in queue)";
            currentOrder.StartedWaitingTime = Simulation.SimulationTime;
            Simulation.PendingVarnishedMaterialsQueue.Enqueue(currentOrder);
        }
        else
        {
            // Pracovník je k dispozícii, pracoovník príde na linku s namoreným a nalakovaným materiálom
            currentOrder.State = "Varnished (waiting for worker B)";
            availableWorker.CurrentOrder = currentOrder;
            
            double arrivalTime;
            
            if (availableWorker.CurrentAssemblyLine == currentAssemblyLine)
            {
                arrivalTime = Simulation.SimulationTime;
                availableWorker.CurrentAssemblyLine?.IdleWorkers.Remove(availableWorker);
            }
            else if (availableWorker.IsInWarehouse)
            {
                arrivalTime = Simulation.SimulationTime + Simulation.ArrivalTimeBetweenLineAndWarehouseGenerator.Next();
                availableWorker.IsInWarehouse = false;
                availableWorker.IsMovingToAssemblyLine = true;
            }
            else if (availableWorker.CurrentAssemblyLine != null)
            {
                arrivalTime = Simulation.SimulationTime + Simulation.ArrivalTimeBetweenTwoLinesGenerator.Next();
                availableWorker.IsMovingToAssemblyLine = true;
                availableWorker.CurrentAssemblyLine?.IdleWorkers.Remove(availableWorker);
            }
            else
            {
                throw new Exception("Undefined position of worker B");
            }
            
            Simulation.AverageWaitingTimeInPendingVarnishedMaterialsQueue.AddValue(0);
            
            var arrivalToLineWithVarnishedMaterial = new ArrivalToLineWithVarnishedMaterial(arrivalTime, Simulation, availableWorker, currentAssemblyLine);
            
            Simulation.ScheduleEvent(arrivalToLineWithVarnishedMaterial);
        }
        
        // Pracovník zo skupiny C je voľný, preto najskôr prednostne
        // skontrolujeme front čakajúcich zložených skríň
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
