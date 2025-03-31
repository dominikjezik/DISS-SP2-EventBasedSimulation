using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class FurnitureFoldingCompleted : FurnitureManufacturerBaseEvent
{
    public FurnitureFoldingCompleted(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker) : base(time, eventSimulationCore, worker)
    {
    }

    public override void Execute()
    {
        // Nábytok bol zložený, ak išlo o skriňu ešte treba namontovať kovanie
        // Ak je k dispozícií pracovník zo skupiny C
        // tak sa presunie k linke s týmto nábytkom
        
        var currentOrder = CurrentWorker.CurrentOrder;
        var currentAssemblyLine = CurrentWorker.CurrentAssemblyLine;
        
        CurrentWorker.CurrentOrder = null;
        
        if (currentOrder.Type != FurnitureType.Closet)
        {
            // Objednávka nábytku je ukončená
            Simulation.AverageProcessingOrderTime.AddValue(Simulation.SimulationTime - currentOrder.ArrivalTime);
            
            currentOrder.State = "Completed";

            Simulation.ReleaseAssemblyLine(currentAssemblyLine);
        }
        else
        {
            var availableWorker = Simulation.GetAvailableWorker(WorkerGroup.GroupC, currentAssemblyLine);
            
            currentAssemblyLine.CurrentWorker = availableWorker;
            
            if (availableWorker == null)
            {
                // Pracovník nie je k dispozícii, skriňa sa pridá do frontu čakajúcich zložených skríň
                currentOrder.State = "Folded (waiting in queue)";
                currentOrder.StartedWaitingTime = Simulation.SimulationTime;
                Simulation.PendingFoldedClosetsQueue.Enqueue(currentOrder);
            }
            else
            {
                // Pracovník je k dispozícii, pracovník príde na linku so zloženou skriňou
                currentOrder.State = "Folded (waiting for worker C)";
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
                    throw new Exception("Undefined position of worker C");
                }
                
                Simulation.AverageWaitingTimeInPendingFoldedClosetsQueue.AddValue(0);
                
                var arrivalToLineWithFoldedCloset = new ArrivalToLineWithFoldedCloset(arrivalTime, Simulation, availableWorker, currentAssemblyLine);
                
                Simulation.ScheduleEvent(arrivalToLineWithFoldedCloset);
            }
        }
        
        // Pracovník zo skupiny B je voľný, preto ak je materiál v čakajúcom fronte namorených a nalakovaných materiálov tak sa spracuje
        if (Simulation.PendingVarnishedMaterialsQueue.Count > 0)
        {
            var pendingVarnishedMaterial = Simulation.PendingVarnishedMaterialsQueue.Dequeue();
            
            CurrentWorker.CurrentOrder = pendingVarnishedMaterial;
            CurrentWorker.IsMovingToAssemblyLine = true;
            
            Simulation.AverageWaitingTimeInPendingVarnishedMaterialsQueue.AddValue(Simulation.SimulationTime - pendingVarnishedMaterial.StartedWaitingTime);
            
            var arrivalTime = Simulation.SimulationTime + Simulation.ArrivalTimeBetweenLineAndWarehouseGenerator.Next();
            var arrivalToLineWithVarnishedMaterial = new ArrivalToLineWithVarnishedMaterial(arrivalTime, Simulation, CurrentWorker, pendingVarnishedMaterial.CurrentAssemblyLine);
            
            Simulation.ScheduleEvent(arrivalToLineWithVarnishedMaterial);
        }
        else
        {
            currentAssemblyLine.IdleWorkers.Add(CurrentWorker);
        }
    }
}
