namespace DiscreteSimulation.Core.Utilities;

public class Statistics
{
    private static readonly double[] StudentsTDistributionCriticalValues = [12.706, 4.303, 3.182, 2.776, 2.571, 2.447, 2.365, 2.306, 2.262, 2.228, 2.201, 2.179, 2.160, 2.145, 2.131, 2.120, 2.110, 2.101, 2.093, 2.086, 2.080, 2.074, 2.069, 2.064, 2.060, 2.056, 2.052, 2.048, 2.045];

    private double _summedValues;
    private double _summedValuesSquared;
    private int _numberOfValues;
    
    public double Mean => _summedValues / _numberOfValues;
    
    public double StandardDeviation => Math.Sqrt((_summedValuesSquared - _summedValues * _summedValues / _numberOfValues) / (_numberOfValues - 1));
    
    public (double, double) ConfidenceInterval95()
    {
        if (_numberOfValues == 0)
        {
            return (double.NaN, double.NaN);
        }
        
        if (_numberOfValues < 30)
        {
            var t = StudentsTDistributionCriticalValues[_numberOfValues - 1];
            var ht = t * StandardDeviation / Math.Sqrt(_numberOfValues);
            
            return (Mean - ht, Mean + ht);
        }
        
        var hz = StandardDeviation * 1.96 / Math.Sqrt(_numberOfValues);
        
        return (Mean - hz, Mean + hz);
    }
    
    public void AddValue(double value)
    {
        _summedValues += value;
        _summedValuesSquared += value * value;
        _numberOfValues++;
    }
    
    public void Clear()
    {
        _summedValues = 0;
        _summedValuesSquared = 0;
        _numberOfValues = 0;
    }
}
