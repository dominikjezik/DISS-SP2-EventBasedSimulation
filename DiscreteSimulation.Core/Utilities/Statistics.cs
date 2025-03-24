namespace DiscreteSimulation.Core.Utilities;

public class Statistics
{
    private double _summedValues;
    private double _summedValuesSquared;
    private int _numberOfValues;
    
    public double Mean => _summedValues / _numberOfValues;
    
    public double StandardDeviation => Math.Sqrt((_summedValuesSquared - _summedValues * _summedValues / _numberOfValues) / (_numberOfValues - 1));
    
    public (double, double) ConfidenceInterval95()
    {
        if (_numberOfValues < 30)
        {
            return (double.NaN, double.NaN);
        }
        
        var h = StandardDeviation * 1.96 / Math.Sqrt(_numberOfValues);
        
        return (Mean - h, Mean + h);
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
