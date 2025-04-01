using DiscreteSimulation.Core.Generators;
using DiscreteSimulation.Core.SimulationCore;
using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.DTOs;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Events;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Simulation;

public class FurnitureManufacturerSimulation : EventSimulationCore
{
    #region Parameters
    
    public int CountOfWorkersGroupA { get; set; }
    
    public int CountOfWorkersGroupB { get; set; }
    
    public int CountOfWorkersGroupC { get; set; }
    
    public bool EnableWorkerLocationPreference { get; set; } = true;

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
    
    public EntitiesQueue<Order> PendingCutMaterialsQueue { get; private set; } 
    
    public EntitiesQueue<Order> PendingVarnishedMaterialsQueue { get; private set; }
    
    public EntitiesQueue<Order> PendingFoldedClosetsQueue { get; private set; }

    #endregion

    #region Entities
    
    public Worker[] WorkersGroupA { get; private set; }
    
    public Worker[] WorkersGroupB { get; private set; }
    
    public Worker[] WorkersGroupC { get; private set; }
    
    public List<AssemblyLine> AssemblyLines { get; private set; } = new();

    public PriorityQueue<AssemblyLine, int> AvailableAssemblyLines = new();
    
    public List<Order> Orders { get; private set; } = new();
    
    #endregion

    #region Statistics

    public Statistics AverageProcessingOrderTime { get; private set; } = new();

    public Statistics AverageWaitingTimeInPendingOrdersQueue { get; private set; } = new();
    
    public Statistics AverageWaitingTimeInPendingCutMaterialsQueue { get; private set; } = new();
    
    public Statistics AverageWaitingTimeInPendingVarnishedMaterialsQueue{ get; private set; } = new();
    
    public Statistics AverageWaitingTimeInPendingFoldedClosetsQueue { get; private set; } = new();
    
    public Statistics SimulationAverageProcessingOrderTime { get; private set; } = new();
    
    public Statistics SimulationAverageWaitingTimeInPendingOrdersQueue { get; private set; } = new();
    
    public Statistics SimulationAverageWaitingTimeInPendingCutMaterialsQueue { get; private set; } = new();
    
    public Statistics SimulationAverageWaitingTimeInPendingVarnishedMaterialsQueue { get; private set; } = new();
    
    public Statistics SimulationAverageWaitingTimeInPendingFoldedClosetsQueue { get; private set; } = new();

    public Statistics SimulationAveragePendingOrdersCount { get; private set; } = new();
    
    public Statistics SimulationAveragePendingCutMaterialsCount { get; private set; } = new();
    
    public Statistics SimulationAveragePendingVarnishedMaterialsCount { get; private set; } = new();
    
    public Statistics SimulationAveragePendingFoldedClosetsCount { get; private set; } = new();
    
    public Statistics SimulationAverageWorkersGroupAUtilization { get; private set; } = new();
    
    public Statistics SimulationAverageWorkersGroupBUtilization { get; private set; } = new();
    
    public Statistics SimulationAverageWorkersGroupCUtilization { get; private set; } = new();
    
    public Statistics[] SimulationAverageAllWorkersUtilization { get; private set; }

    #endregion

    public FurnitureManufacturerSimulation()
    {
        PendingOrdersQueue = new EntitiesQueue<Order>(this);
        PendingCutMaterialsQueue = new EntitiesQueue<Order>(this);
        PendingVarnishedMaterialsQueue = new EntitiesQueue<Order>(this);
        PendingFoldedClosetsQueue = new EntitiesQueue<Order>(this);
    }

    public override void BeforeSimulation(int? seedForSeedGenerator = null)
    {
        base.BeforeSimulation(seedForSeedGenerator);
        
        InitializeGenerators();
        
        SimulationAverageProcessingOrderTime.Clear();
        
        SimulationAverageWaitingTimeInPendingOrdersQueue.Clear();
        SimulationAverageWaitingTimeInPendingCutMaterialsQueue.Clear();
        SimulationAverageWaitingTimeInPendingVarnishedMaterialsQueue.Clear();
        SimulationAverageWaitingTimeInPendingFoldedClosetsQueue.Clear();
        
        SimulationAveragePendingOrdersCount.Clear();
        SimulationAveragePendingCutMaterialsCount.Clear();
        SimulationAveragePendingVarnishedMaterialsCount.Clear();
        SimulationAveragePendingFoldedClosetsCount.Clear();
        SimulationAverageWorkersGroupAUtilization.Clear();
        SimulationAverageWorkersGroupBUtilization.Clear();
        SimulationAverageWorkersGroupCUtilization.Clear();
        
        WorkersGroupA = new Worker[CountOfWorkersGroupA];
        WorkersGroupB = new Worker[CountOfWorkersGroupB];
        WorkersGroupC = new Worker[CountOfWorkersGroupC];
        
        SimulationAverageAllWorkersUtilization = new Statistics[CountOfWorkersGroupA + CountOfWorkersGroupB + CountOfWorkersGroupC];
        
        for (var i = 0; i < SimulationAverageAllWorkersUtilization.Length; i++)
        {
            SimulationAverageAllWorkersUtilization[i] = new Statistics();
        }
    }

    public override void BeforeReplication()
    {
        base.BeforeReplication();
        
        PendingOrdersQueue.Clear();
        PendingCutMaterialsQueue.Clear();
        PendingVarnishedMaterialsQueue.Clear();
        PendingFoldedClosetsQueue.Clear();
        
        Orders.Clear();
        AssemblyLines.Clear();
        AvailableAssemblyLines.Clear();
        InitializeWorkers();
        
        AverageProcessingOrderTime.Clear();
        AverageWaitingTimeInPendingOrdersQueue.Clear();
        AverageWaitingTimeInPendingCutMaterialsQueue.Clear();
        AverageWaitingTimeInPendingVarnishedMaterialsQueue.Clear();
        AverageWaitingTimeInPendingFoldedClosetsQueue.Clear();
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
        
        // Ak bolo vykonávanie replikácie prerušené zastavením simulácie pouzívateľom
        // výsledky tejto replikácie sa nezapočítavajú do štatistík
        if (IsReplicationStopped)
        {
            return;
        }
        
        SimulationAverageProcessingOrderTime.AddValue(AverageProcessingOrderTime.Mean);
        
        SimulationAverageWaitingTimeInPendingOrdersQueue.AddValue(AverageWaitingTimeInPendingOrdersQueue.Mean);
        SimulationAverageWaitingTimeInPendingCutMaterialsQueue.AddValue(AverageWaitingTimeInPendingCutMaterialsQueue.Mean);
        SimulationAverageWaitingTimeInPendingVarnishedMaterialsQueue.AddValue(AverageWaitingTimeInPendingVarnishedMaterialsQueue.Mean);
        SimulationAverageWaitingTimeInPendingFoldedClosetsQueue.AddValue(AverageWaitingTimeInPendingFoldedClosetsQueue.Mean);
        
        PendingOrdersQueue.RefreshStatistics();
        SimulationAveragePendingOrdersCount.AddValue(PendingOrdersQueue.AverageQueueLength);
        SimulationAveragePendingCutMaterialsCount.AddValue(PendingCutMaterialsQueue.AverageQueueLength);
        SimulationAveragePendingVarnishedMaterialsCount.AddValue(PendingVarnishedMaterialsQueue.AverageQueueLength);
        SimulationAveragePendingFoldedClosetsCount.AddValue(PendingFoldedClosetsQueue.AverageQueueLength);
        
        for (var i = 0; i < WorkersGroupA.Length; i++)
        {
            WorkersGroupA[i].RefreshStatistics();
            SimulationAverageAllWorkersUtilization[i].AddValue(WorkersGroupA[i].Utilization);
        }
        
        for (var i = 0; i < WorkersGroupB.Length; i++)
        {
            WorkersGroupB[i].RefreshStatistics();
            SimulationAverageAllWorkersUtilization[WorkersGroupA.Length + i].AddValue(WorkersGroupB[i].Utilization);
        }
        
        for (var i = 0; i < WorkersGroupC.Length; i++)
        {
            WorkersGroupC[i].RefreshStatistics();
            SimulationAverageAllWorkersUtilization[WorkersGroupA.Length + WorkersGroupB.Length + i].AddValue(WorkersGroupC[i].Utilization);
        }
        
        SimulationAverageWorkersGroupAUtilization.AddValue(WorkersGroupA.Average(worker => worker.Utilization));
        SimulationAverageWorkersGroupBUtilization.AddValue(WorkersGroupB.Average(worker => worker.Utilization));
        SimulationAverageWorkersGroupCUtilization.AddValue(WorkersGroupC.Average(worker => worker.Utilization));
    }

    public Worker? GetAvailableWorker(WorkerGroup workerGroup, AssemblyLine? preferredAssemblyLine)
    {
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

        // Ak je vypnuté preferovanie voľných pracovníkov na základe ich polohy,
        // vrátime prvého voľného pracovníka
        if (!EnableWorkerLocationPreference)
        {
            return workers.FirstOrDefault(worker => !worker.IsBusy);
        }
        
        Worker? availableWorker = null;
        Worker? availableWorkerFromWarehouse = null;

        // Preferujeme najskôr pracovníka, ktorý sa už na danej linke nachádza,
        // inak pracovníka, ktorý je v sklade (E(X) príchodu zo skladu je menší
        // ako E(X) príchodu z inej linky) inak iný voľný pracovník
        foreach (var worker in workers)
        {
            if (!worker.IsBusy && availableWorker == null)
            {
                availableWorker = worker;
            }
            
            if (!worker.IsBusy && worker.CurrentAssemblyLine == preferredAssemblyLine)
            {
                return worker;
            }
            
            if (!worker.IsBusy && worker.IsInWarehouse && availableWorkerFromWarehouse == null)
            {
                availableWorkerFromWarehouse = worker;
            }
        }

        return availableWorkerFromWarehouse ?? availableWorker;
    }

    public AssemblyLine RequestFreeAssemblyLine()
    {
        if (AvailableAssemblyLines.Count > 0)
        {
            return AvailableAssemblyLines.Dequeue();
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
    
    public void ReleaseAssemblyLine(AssemblyLine assemblyLine)
    {
        assemblyLine.CurrentOrder.CurrentAssemblyLine = null;
        assemblyLine.CurrentOrder = null;
        assemblyLine.CurrentWorker = null;
        
        AvailableAssemblyLines.Enqueue(assemblyLine, assemblyLine.Id);
    }

    public List<OrderDTO> GetCurrentOrderDTOs()
    {
        return Orders.Select(order => order.ToDTO(SimulationTime)).ToList();
    }
    
    public List<AssemblyLineDTO> GetCurrentAssemblyLineDTOs()
    {
        return AssemblyLines.Select(assemblyLine => assemblyLine.ToDTO()).ToList();
    }
    
    public List<WorkerDTO> GetCurrentWorkerGroupADTOs()
    {
        return WorkersGroupA.Select(worker => worker.ToDTO()).ToList();
    }
    
    public List<WorkerDTO> GetCurrentWorkerGroupBDTOs()
    {
        return WorkersGroupB.Select(worker => worker.ToDTO()).ToList();
    }
    
    public List<WorkerDTO> GetCurrentWorkerGroupCDTOs()
    {
        return WorkersGroupC.Select(worker => worker.ToDTO()).ToList();
    }
    
    public List<OrderDTO> GetCurrentPendingOrdersQueue()
    {
        return PendingOrdersQueue.OriginalQueue.Select(order => order.ToDTO(SimulationTime)).ToList();
    }
    
    public List<OrderDTO> GetCurrentPendingCutMaterialsQueue()
    {
        return PendingCutMaterialsQueue.OriginalQueue.Select(order => order.ToDTO(SimulationTime)).ToList();
    }
    
    public List<OrderDTO> GetCurrentPendingVarnishedMaterialsQueue()
    {
        return PendingVarnishedMaterialsQueue.OriginalQueue.Select(order => order.ToDTO(SimulationTime)).ToList();
    }
    
    public List<OrderDTO> GetCurrentPendingFoldedClosetsQueue()
    {
        return PendingFoldedClosetsQueue.OriginalQueue.Select(order => order.ToDTO(SimulationTime)).ToList();
    }
    
    public List<WorkerDTO> GetAllWorkersSimulationUtilization()
    {
        var workers = new List<WorkerDTO>();
        
        for (int i = 0; i < SimulationAverageAllWorkersUtilization.Length; i++)
        {
            var utilization = SimulationAverageAllWorkersUtilization[i].ConfidenceInterval95();
            var mean = SimulationAverageAllWorkersUtilization[i].Mean;
            
            WorkerDTO? worker;
            
            if (i < WorkersGroupA.Length)
            {
                worker = WorkersGroupA[i].ToUtilizationDTO(mean, utilization);
            }
            else if (i < WorkersGroupA.Length + WorkersGroupB.Length)
            {
                worker = WorkersGroupB[i - WorkersGroupA.Length].ToUtilizationDTO(mean, utilization);
            }
            else
            {
                worker = WorkersGroupC[i - WorkersGroupA.Length - WorkersGroupB.Length].ToUtilizationDTO(mean, utilization);
            }
            
            workers.Add(worker);
        }

        return workers;
    }

    private void InitializeWorkers()
    {
        for (var i = 0; i < CountOfWorkersGroupA; i++)
        {
            WorkersGroupA[i] = new Worker(this)
            {
                Id = i + 1,
                Group = WorkerGroup.GroupA,
                IsInWarehouse = true
            };
        }
        
        for (var i = 0; i < CountOfWorkersGroupB; i++)
        {
            WorkersGroupB[i] = new Worker(this)
            {
                Id = i + 1,
                Group = WorkerGroup.GroupB,
                IsInWarehouse = true
            };
        }

        for (var i = 0; i < CountOfWorkersGroupC; i++)
        {
            WorkersGroupC[i] = new Worker(this)
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
