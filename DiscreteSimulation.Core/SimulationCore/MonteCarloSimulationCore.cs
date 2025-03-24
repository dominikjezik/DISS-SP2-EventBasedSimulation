using DiscreteSimulation.Core.Generators;

namespace DiscreteSimulation.Core.SimulationCore;

public abstract class MonteCarloSimulationCore
{
    public bool IsSimulationRunning { get; private set; } = false;
    
    public long CurrentReplication { get; private set; } = 0;
    
    public long CurrentMaxReplications { get; private set; } = 0;
    
    protected SeedGenerator SeedGenerator { get; private set; }

    public virtual void BeforeSimulation(int? seedForSeedGenerator = null)
    {
        if (seedForSeedGenerator == null)
        {
            SeedGenerator = new SeedGenerator();
        }
        else
        {
            SeedGenerator = new SeedGenerator(seedForSeedGenerator.Value);
        }
    }
    
    public virtual void BeforeReplication()
    {
    }
    
    public void StartSimulation(long replications)
    {
        CurrentMaxReplications = replications;
        IsSimulationRunning = true;
        
        BeforeSimulation();
        
        for (CurrentReplication = 1; CurrentReplication <= replications; CurrentReplication++)
        {
            if (!IsSimulationRunning)
            {
                break;
            }
            
            BeforeReplication();
            
            ExecuteReplication();
            
            AfterReplication();
            
            ReplicationEnded?.Invoke();
        }
        
        AfterSimulation();
        
        IsSimulationRunning = false;
        
        SimulationEnded?.Invoke();
    }
    
    public abstract void ExecuteReplication();

    public virtual void AfterReplication()
    {
    }
    
    public event Action? ReplicationEnded;
    
    public virtual void AfterSimulation()
    {
    }
    
    public event Action? SimulationEnded;
    
    public void StopSimulation()
    {
        IsSimulationRunning = false;
    }
}
