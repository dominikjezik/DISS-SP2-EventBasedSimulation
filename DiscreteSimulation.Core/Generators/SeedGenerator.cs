namespace DiscreteSimulation.Core.Generators;

public class SeedGenerator
{
    private readonly Random _random;
    
    public SeedGenerator()
    {
        _random = new Random();
    }
    
    public SeedGenerator(int seed)
    {
        _random = new Random(seed);
    }
    
    public int Next()
    {
        var seed = _random.Next();
        return seed;
    }
}
