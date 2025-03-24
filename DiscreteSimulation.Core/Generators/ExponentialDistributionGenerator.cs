namespace DiscreteSimulation.Core.Generators;

public class ExponentialDistributionGenerator
{
    private readonly Random _random;
    
    public double Lambda { get; }

    public ExponentialDistributionGenerator(double lambda, int seed)
    {
        Lambda = lambda;
        _random = new Random(seed);
    }
    
    public double Next()
    {
        return -Math.Log(1 - _random.NextDouble()) / Lambda;
    }
}
