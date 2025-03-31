using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public class NewOrderArrival : FurnitureManufacturerBaseEvent
{
    private int _orderCounter = 0;
    
    public NewOrderArrival(double time, FurnitureManufacturerSimulation eventSimulationCore) : base(time, eventSimulationCore)
    {
    }

    public override void Execute()
    {
        var newOrder = GenerateOrder();
        
        Simulation.Orders.Add(newOrder);
        
        // Získanie volného pracovníka zo skupiny A
        var availableWorker = Simulation.GetAvailableWorker(WorkerGroup.GroupA, null);

        if (availableWorker == null)
        {
            // Pracovník nie je k dispozícii, objednávka sa pridá do frontu čakajúcich objednávok
            Simulation.PendingOrdersQueue.Enqueue(newOrder);
        }
        else
        {
            availableWorker.CurrentOrder = newOrder;
            
            Simulation.AverageWaitingTimeInPendingOrdersQueue.AddValue(0);
            
            var startOfOrderPreparation = new StartOfOrderPreparation(Simulation.SimulationTime, Simulation, availableWorker);
            
            Simulation.ScheduleEvent(startOfOrderPreparation);
        }
        
        // Naplánovanie ďalšieho príchodu objednávky
        Time = Simulation.SimulationTime + Simulation.NewOrderArrivalGenerator.Next();
        Simulation.ScheduleEvent(this);
    }
    
    private Order GenerateOrder()
    {
        FurnitureType furnitureType;
        
        var orderTypeProbability = Simulation.OrderTypeGenerator.Next();
        
        if (orderTypeProbability < 0.5)
        {
            furnitureType = FurnitureType.Desk;
        }
        else if (orderTypeProbability < 0.65)
        {
            furnitureType = FurnitureType.Chair;
        }
        else
        {
            furnitureType = FurnitureType.Closet;
        }
        
        return new Order
        {
            Id = ++_orderCounter,
            Type = furnitureType,
            State = "Pending",
            ArrivalTime = Time,
            StartedWaitingTime = Time
        };
    }
}
