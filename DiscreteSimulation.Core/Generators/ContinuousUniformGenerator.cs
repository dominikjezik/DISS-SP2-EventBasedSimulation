namespace DiscreteSimulation.Core.Generators;

public class ContinuousUniformGenerator
{
    private readonly Random _random;
    
    public double LowerBound { get; set; }
    
    public double UpperBound { get; set; }
    
    public ContinuousUniformGenerator(double lowerBound, double upperBound, int seed)
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;
        
        _random = new Random(seed);
    }
    
    public double Next()
    {
        return _random.NextDouble() * (UpperBound - LowerBound) + LowerBound;
    }
}
