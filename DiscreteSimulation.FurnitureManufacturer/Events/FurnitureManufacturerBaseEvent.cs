using DiscreteSimulation.Core.Events;
using DiscreteSimulation.Core.SimulationCore;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Events;

public abstract class FurnitureManufacturerBaseEvent : BaseEvent
{
    protected Worker CurrentWorker { get; set; }
    
    protected FurnitureManufacturerSimulation Simulation { get; private set; }
    
    protected FurnitureManufacturerBaseEvent(double time, FurnitureManufacturerSimulation eventSimulationCore) : base(time, eventSimulationCore)
    {
        Simulation = eventSimulationCore;
    }
    
    protected FurnitureManufacturerBaseEvent(double time, FurnitureManufacturerSimulation eventSimulationCore, Worker worker) : base(time, eventSimulationCore)
    {
        Simulation = eventSimulationCore;
        CurrentWorker = worker;
    }
}
