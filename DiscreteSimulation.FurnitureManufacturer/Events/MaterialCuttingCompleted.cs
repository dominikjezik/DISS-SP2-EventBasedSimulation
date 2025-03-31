using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class MaterialCuttingCompleted : FurnitureManufacturerBaseEvent
{
    public MaterialCuttingCompleted(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker) : base(time, eventSimulationCore, worker)
    {
    }

    public override void Execute()
    {
        // Material bol narezaný, ak je k dispozícií pracovník zo skupiny C
        // tak sa presunie k linke s týmto materiálom
        
        var currentOrder = CurrentWorker.CurrentOrder;
        var currentAssemblyLine = CurrentWorker.CurrentAssemblyLine;
        
        CurrentWorker.CurrentOrder = null;
        
        var availableWorker = Simulation.GetAvailableWorker(WorkerGroup.GroupC, currentAssemblyLine);
        
        currentAssemblyLine.CurrentWorker = availableWorker;
        
        if (availableWorker == null)
        {
            // Pracovník nie je k dispozícii, materiál sa pridá do frontu čakajúcich narezaných materiálov
            currentOrder.State = "Material cut (in queue)";
            currentOrder.StartedWaitingTime = Simulation.SimulationTime;
            Simulation.PendingCutMaterialsQueue.Enqueue(currentOrder);
        }
        else
        {
            // Pracovník je k dispozícii, pracovník príde na linku s narezaným materiálom
            currentOrder.State = "Material cut (waiting for worker C)";
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
            
            Simulation.AverageWaitingTimeInPendingCutMaterialsQueue.AddValue(0);

            var arrivalToLineWithCutMaterial = new ArrivalToLineWithCutMaterial(arrivalTime, Simulation, availableWorker, currentAssemblyLine);
            
            Simulation.ScheduleEvent(arrivalToLineWithCutMaterial);
        }
        
        // Pracovnik zo skupiny A je voľný, preto skontrolujeme, či je nejaká čakajúca objednávka pre spracovanie
        if (Simulation.PendingOrdersQueue.Count > 0)
        {
            var order = Simulation.PendingOrdersQueue.Dequeue();
            
            CurrentWorker.CurrentOrder = order;
            
            Simulation.AverageWaitingTimeInPendingOrdersQueue.AddValue(Simulation.SimulationTime - order.StartedWaitingTime);
            
            var startOfOrderPreparation = new StartOfOrderPreparation(Simulation.SimulationTime, Simulation, CurrentWorker);
            
            Simulation.ScheduleEvent(startOfOrderPreparation);
        }
        else
        {
            currentAssemblyLine.IdleWorkers.Add(CurrentWorker);
        }
    }
}
