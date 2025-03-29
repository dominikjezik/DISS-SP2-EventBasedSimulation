using DiscreteSimulation.Core.SimulationCore;

namespace DiscreteSimulation.Core.Utilities;

public class EntitiesQueue<TEntity>
{
    private EventSimulationCore _eventSimulationCore;
    
    private Queue<TEntity> _queue = new();
    private WeightedStatistics _weightedStatistics = new();
    private double _lastChangeInQueueTime = 0;
    
    public Queue<TEntity> OriginalQueue => _queue;
    
    public int Count => _queue.Count;
    
    public double AverageQueueLength => double.IsNaN(_weightedStatistics.Mean) ? 0 : _weightedStatistics.Mean;
    
    public EntitiesQueue(EventSimulationCore eventSimulationCore)
    {
        _eventSimulationCore = eventSimulationCore;
    }
    
    public void Enqueue(TEntity entity)
    {
        RefreshStatistics();
        _queue.Enqueue(entity);
    }
    
    public TEntity Dequeue()
    {
        RefreshStatistics();
        return _queue.Dequeue();
    }
    
    public void Clear()
    {
        _queue.Clear();
        _weightedStatistics.Clear();
        _lastChangeInQueueTime = 0;
    }

    public void RefreshStatistics()
    {
        var timeInterval = _eventSimulationCore.SimulationTime - _lastChangeInQueueTime;
        var queueLength = _queue.Count;
        
        _weightedStatistics.AddValue(queueLength, timeInterval);
        
        _lastChangeInQueueTime = _eventSimulationCore.SimulationTime;
    }
}
