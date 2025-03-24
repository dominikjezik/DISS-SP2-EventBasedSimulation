namespace DiscreteSimulation.Core.Utilities;

public class WeightedStatistics
{
    private double _summedValuesWithWeights;
    private double _summedWeights;
    
    public double Mean => _summedValuesWithWeights / _summedWeights;
    
    public void AddValue(double value, double weight)
    {
        _summedValuesWithWeights += value * weight;
        _summedWeights += weight;
    }
    
    public void Clear()
    {
        _summedValuesWithWeights = 0;
        _summedWeights = 0;
    }
}
