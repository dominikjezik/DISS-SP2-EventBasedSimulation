using DiscreteSimulation.Core.Generators;
using DiscreteSimulation.Core.SimulationCore;
using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Events;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Simulation;

public class FurnitureManufacturerSimulation : EventSimulationCore
{
    #region Parameters
    
    public int CountOfWorkersGroupA { get; set; } = 3;
    
    public int CountOfWorkersGroupB { get; set; } = 5;
    
    public int CountOfWorkersGroupC { get; set; } = 4;

    #endregion
    
    #region Generators

    public ExponentialDistributionGenerator NewOrderArrivalGenerator { get; private set; }
    
    public ContinuousUniformGenerator OrderTypeGenerator { get; private set; }
    
    public TriangularDistributionGenerator ArrivalTimeBetweenLineAndWarehouseGenerator { get; private set; }
    
    public TriangularDistributionGenerator ArrivalTimeBetweenTwoLinesGenerator { get; private set; }
    
    public TriangularDistributionGenerator MaterialPreparationTimeGenerator { get; private set; }
    
    public EmpiricalProbabilityGenerator CuttingDeskTimeGenerator { get; private set; }
    
    public ContinuousUniformGenerator CuttingChairTimeGenerator { get; private set; }
    
    public ContinuousUniformGenerator CuttingClosetTimeGenerator { get; private set; }
    
    public ContinuousUniformGenerator VarnishingDeskTimeGenerator { get; private set; }
    
    public ContinuousUniformGenerator VarnishingChairTimeGenerator { get; private set; }
    
    public ContinuousUniformGenerator VarnishingClosetTimeGenerator { get; private set; }
    
    public ContinuousUniformGenerator FoldingDeskTimeGenerator { get; private set; }
    
    public ContinuousUniformGenerator FoldingChairTimeGenerator { get; private set; }
    
    public ContinuousUniformGenerator FoldingClosetTimeGenerator { get; private set; }
    
    public ContinuousUniformGenerator AssemblyOfFittingsOnClosetTimeGenerator { get; private set; }

    #endregion

    #region Queues

    public EntitiesQueue<Order> PendingOrdersQueue { get; private set; }
    
    public Queue<Order> PendingCutMaterialsQueue { get; private set; } 
    
    public Queue<Order> PendingVarnishedMaterialsQueue { get; private set; }
    
    public Queue<Order> PendingFoldedClosetsQueue { get; private set; }

    #endregion

    #region Entities
    
    public Worker[] WorkersGroupA { get; private set; }
    
    public Worker[] WorkersGroupB { get; private set; }
    
    public Worker[] WorkersGroupC { get; private set; }
    
    public List<AssemblyLine> AssemblyLines { get; private set; } = new();
    
    public List<Order> UnfinishedOrders { get; private set; } = new();
    
    #endregion

    #region Statistics

    public Statistics AverageProcessingOrderTime { get; private set; } = new();
    
    public Statistics SimulationAverageProcessingOrderTime { get; private set; } = new();

    #endregion

    public FurnitureManufacturerSimulation()
    {
        PendingOrdersQueue = new EntitiesQueue<Order>(this);
        PendingCutMaterialsQueue = new Queue<Order>();
        PendingVarnishedMaterialsQueue = new Queue<Order>();
        PendingFoldedClosetsQueue = new Queue<Order>();
    }

    public override void BeforeSimulation(int? seedForSeedGenerator = null)
    {
        base.BeforeSimulation(seedForSeedGenerator);
        
        InitializeGenerators();
        
        SimulationAverageProcessingOrderTime.Clear();
    }

    public override void BeforeReplication()
    {
        base.BeforeReplication();
        
        PendingOrdersQueue.Clear();
        PendingCutMaterialsQueue.Clear();
        PendingVarnishedMaterialsQueue.Clear();
        PendingFoldedClosetsQueue.Clear();
        
        UnfinishedOrders.Clear();
        AssemblyLines.Clear();
        InitializeWorkers();
        
        AverageProcessingOrderTime.Clear();
    }
    
    public override void ExecuteReplication()
    {
        // Naplanovanie prveho prichodu objednavky
        var firstOrderArrival = new NewOrderArrival(0, this);
        ScheduleEvent(firstOrderArrival);
        
        // Spustenie spracovavania udalosti
        base.ExecuteReplication();
    }

    public override void AfterReplication()
    {
        base.AfterReplication();
        
        // TODO: Ak replikacia nedobehla cela (simulacia bola predcasne ukoncena), tak sa nepridavaju hodnoty do statistik
        
        SimulationAverageProcessingOrderTime.AddValue(AverageProcessingOrderTime.Mean);
    }

    public Worker? GetAvailableWorker(WorkerGroup workerGroup)
    {
        // TODO: Preferencia na aktuálnu pozíciu pracovníka
        
        Worker[] workers;
        
        switch (workerGroup)
        {
            case WorkerGroup.GroupA:
                workers = WorkersGroupA;
                break;
            case WorkerGroup.GroupB:
                workers = WorkersGroupB;
                break;
            case WorkerGroup.GroupC:
                workers = WorkersGroupC;
                break;
            default:
                throw new ArgumentException("Invalid worker group");
        }
        
        return workers.FirstOrDefault(worker => !worker.IsBusy);
    }

    public AssemblyLine RequestFreeAssemblyLine()
    {
        foreach (var assemblyLine in AssemblyLines)
        {
            if (assemblyLine.CurrentOrder == null)
            {
                return assemblyLine;
            }
        }
        
        var newAssemblyLine = new AssemblyLine
        {
            Id = AssemblyLines.Count + 1,
            CurrentOrder = null,
            CurrentWorker = null
        };
        
        AssemblyLines.Add(newAssemblyLine);
        
        return newAssemblyLine;
    }

    public void InitializeWorkers()
    {
        // Delete all workers
        WorkersGroupA = new Worker[CountOfWorkersGroupA];
        WorkersGroupB = new Worker[CountOfWorkersGroupB];
        WorkersGroupC = new Worker[CountOfWorkersGroupC];
        
        // Initialize workers
        for (var i = 0; i < CountOfWorkersGroupA; i++)
        {
            WorkersGroupA[i] = new Worker
            {
                Id = i + 1,
                Group = WorkerGroup.GroupA,
                IsInWarehouse = true
            };
        }
        
        for (var i = 0; i < CountOfWorkersGroupB; i++)
        {
            WorkersGroupB[i] = new Worker
            {
                Id = i + 1,
                Group = WorkerGroup.GroupB,
                IsInWarehouse = true
            };
        }

        for (var i = 0; i < CountOfWorkersGroupC; i++)
        {
            WorkersGroupC[i] = new Worker
            {
                Id = i + 1,
                Group = WorkerGroup.GroupC,
                IsInWarehouse = true
            };
        }
    }

    private void InitializeGenerators()
    {
        NewOrderArrivalGenerator = new ExponentialDistributionGenerator(1.0 / (60 * 30), SeedGenerator.Next());
        
        OrderTypeGenerator = new ContinuousUniformGenerator(0, 1, SeedGenerator.Next());
        
        ArrivalTimeBetweenLineAndWarehouseGenerator = new TriangularDistributionGenerator(60, 480, 120, SeedGenerator.Next());
        
        ArrivalTimeBetweenTwoLinesGenerator = new TriangularDistributionGenerator(120, 500, 150, SeedGenerator.Next());
        
        MaterialPreparationTimeGenerator = new TriangularDistributionGenerator(300, 900, 500, SeedGenerator.Next());
        
        CuttingDeskTimeGenerator = new EmpiricalProbabilityGenerator(
            isDiscrete: false,
            [
                new EmpiricalProbabilityTableItem(10 * 60, 25 * 60, 0.6),
                new EmpiricalProbabilityTableItem(25 * 60, 50 * 60, 0.4),
            ],
            SeedGenerator
        );
        
        CuttingChairTimeGenerator = new ContinuousUniformGenerator(12 * 60, 16 * 60, SeedGenerator.Next());
        
        CuttingClosetTimeGenerator = new ContinuousUniformGenerator(15 * 60, 80 * 60, SeedGenerator.Next());
        
        VarnishingDeskTimeGenerator = new ContinuousUniformGenerator(200 * 60, 610 * 60, SeedGenerator.Next());
        
        VarnishingChairTimeGenerator = new ContinuousUniformGenerator(210 * 60, 540 * 60, SeedGenerator.Next());
        
        VarnishingClosetTimeGenerator = new ContinuousUniformGenerator(600 * 60, 700 * 60, SeedGenerator.Next());
        
        FoldingDeskTimeGenerator = new ContinuousUniformGenerator(30 * 60, 60 * 60, SeedGenerator.Next());
        
        FoldingChairTimeGenerator = new ContinuousUniformGenerator(14 * 60, 24 * 60, SeedGenerator.Next());
        
        FoldingClosetTimeGenerator = new ContinuousUniformGenerator(35 * 60, 75 * 60, SeedGenerator.Next());
        
        AssemblyOfFittingsOnClosetTimeGenerator = new ContinuousUniformGenerator(15 * 60, 25 * 60, SeedGenerator.Next());
    }
}
