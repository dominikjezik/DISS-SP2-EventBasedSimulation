using DiscreteSimulation.FurnitureManufacturer.Simulation;

namespace DiscreteSimulation.Console;

public class ConfigurationAnalyzer
{
    private readonly FurnitureManufacturerSimulation _simulation = new();
    
    private int _replications;

    private readonly string _basePath;
    
    private string _csvFilePath = string.Empty;

    public int[,] Configurations { get; set; } = new int[3, 3];
    
    public ConfigurationAnalyzer(string basePath)
    {
        _basePath = basePath;
        _simulation.SimulationEnded += PrintSimulationResults;
    }
    
    public void AnalyzeConfigurations(int replications)
    {
        _replications = replications;

        _csvFilePath = Path.Combine(_basePath, "SimulationResults_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv");
        
        // CSV file header
        File.WriteAllText(_csvFilePath, "WorkersA;WorkersB;WorkersC;" +
                                        "ProcessingTime;ProcessingTimeCI_Lower;ProcessingTimeCI_Upper;" +
                                        "PendingOrders;PendingOrdersCI_Lower;PendingOrdersCI_Upper;" +
                                        "PendingCutMaterials;PendingCutMaterialsCI_Lower;PendingCutMaterialsCI_Upper;" +
                                        "PendingVarnishedMaterials;PendingVarnishedMaterialsCI_Lower;PendingVarnishedMaterialsCI_Upper;" +
                                        "PendingFoldedClosets;PendingFoldedClosetsCI_Lower;PendingFoldedClosetsCI_Upper;" +
                                        "WorkersAUtilization;WorkersAUtilizationCI_Lower;WorkersAUtilizationCI_Upper;" +
                                        "WorkersBUtilization;WorkersBUtilizationCI_Lower;WorkersBUtilizationCI_Upper;" +
                                        "WorkersCUtilization;WorkersCUtilizationCI_Lower;WorkersCUtilizationCI_Upper;\n");

        
        for (int i = 0; i < Configurations.GetLength(0); i++)
        {
            AnalyzeConfiguration(Configurations[i, 0], Configurations[i, 1], Configurations[i, 2]);
        }
    }

    public void AnalyzeConfiguration(int workersGroupA, int workersGroupB, int workersGroupC)
    {
        _simulation.CountOfWorkersGroupA = workersGroupA;
        _simulation.CountOfWorkersGroupB = workersGroupB;
        _simulation.CountOfWorkersGroupC = workersGroupC;

        _simulation.MaxReplicationTime = 7_171_200;
        
        _simulation.StartSimulation(_replications);
    }
    
    private void PrintSimulationResults()
    {
        // Console print
        System.Console.WriteLine($"Simulation results for configuration: A={_simulation.CountOfWorkersGroupA}, B={_simulation.CountOfWorkersGroupB}, C={_simulation.CountOfWorkersGroupC}");

        var processingOrderTime = _simulation.SimulationAverageProcessingOrderTime.Mean;
        var processingOrderTimeCI = _simulation.SimulationAverageProcessingOrderTime.ConfidenceInterval95();
        
        System.Console.WriteLine($"Average processing time: {processingOrderTime} - <{processingOrderTimeCI.Item1} ; {processingOrderTimeCI.Item2}> seconds");
        System.Console.WriteLine();
        
        var pendingOrders = _simulation.SimulationAveragePendingOrdersCount.Mean;
        var pendingOrdersCI = _simulation.SimulationAveragePendingOrdersCount.ConfidenceInterval95();
        
        var pendingCutMaterials = _simulation.SimulationAveragePendingCutMaterialsCount.Mean;
        var pendingCutMaterialsCI = _simulation.SimulationAveragePendingCutMaterialsCount.ConfidenceInterval95();
        
        var pendingVarnishedMaterials = _simulation.SimulationAveragePendingVarnishedMaterialsCount.Mean;
        var pendingVarnishedMaterialsCI = _simulation.SimulationAveragePendingVarnishedMaterialsCount.ConfidenceInterval95();
        
        var pendingFoldedClosets = _simulation.SimulationAveragePendingFoldedClosetsCount.Mean;
        var pendingFoldedClosetsCI = _simulation.SimulationAveragePendingFoldedClosetsCount.ConfidenceInterval95();
        
        var workersAUtilization = _simulation.SimulationAverageWorkersGroupAUtilization.Mean;
        var workersAUtilizationCI = _simulation.SimulationAverageWorkersGroupAUtilization.ConfidenceInterval95();
        
        var workersBUtilization = _simulation.SimulationAverageWorkersGroupBUtilization.Mean;
        var workersBUtilizationCI = _simulation.SimulationAverageWorkersGroupBUtilization.ConfidenceInterval95();
        
        var workersCUtilization = _simulation.SimulationAverageWorkersGroupCUtilization.Mean;
        var workersCUtilizationCI = _simulation.SimulationAverageWorkersGroupCUtilization.ConfidenceInterval95();
        
        // CSV file print
        File.AppendAllText(_csvFilePath, $"{_simulation.CountOfWorkersGroupA};{_simulation.CountOfWorkersGroupB};{_simulation.CountOfWorkersGroupC};" +
                                         $"{processingOrderTime};{processingOrderTimeCI.Item1};{processingOrderTimeCI.Item2};" +
                                         $"{pendingOrders};{pendingOrdersCI.Item1};{pendingOrdersCI.Item2};" +
                                         $"{pendingCutMaterials};{pendingCutMaterialsCI.Item1};{pendingCutMaterialsCI.Item2};" +
                                         $"{pendingVarnishedMaterials};{pendingVarnishedMaterialsCI.Item1};{pendingVarnishedMaterialsCI.Item2};" +
                                         $"{pendingFoldedClosets};{pendingFoldedClosetsCI.Item1};{pendingFoldedClosetsCI.Item2};" +
                                         $"{workersAUtilization};{workersAUtilizationCI.Item1};{workersAUtilizationCI.Item2};" +
                                         $"{workersBUtilization};{workersBUtilizationCI.Item1};{workersBUtilizationCI.Item2};" +
                                         $"{workersCUtilization};{workersCUtilizationCI.Item1};{workersCUtilizationCI.Item2};\n");
    }
}
