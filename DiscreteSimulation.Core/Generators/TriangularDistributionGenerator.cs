namespace DiscreteSimulation.Core.Generators;

public class TriangularDistributionGenerator
{
    private readonly Random _random;
    
    public double Min { get; }
    
    public double Max { get; }
    
    public double Modus { get; }

    public TriangularDistributionGenerator(double min, double max, double modus, int seed)
    {
        Min = min;
        Max = max;
        Modus = modus;
        
        _random = new Random(seed);
    }
    
    public double Next()
    {
        var uniform = _random.NextDouble();
        var Fmod = (Modus - Min) / (Max - Min);

        if (uniform < Fmod)
        {
            return Min + Math.Sqrt(uniform * (Max - Min) * (Modus - Min));
        }

        return Max - Math.Sqrt((1 - uniform) * (Max - Min) * (Max - Modus));
    }
}
