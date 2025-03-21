namespace DiscreteSimulation.Core.Generators;

public class DiscreteUniformGenerator
{
    private readonly Random _random;
    
    public int LowerBound { get; set; }
    
    public int UpperBound { get; set; }
    
    public DiscreteUniformGenerator(int lowerBound, int upperBound, int seed)
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;
        
        _random = new Random(seed);
    }
    
    public int Next()
    {
        return _random.Next(LowerBound, UpperBound);
    }
}
