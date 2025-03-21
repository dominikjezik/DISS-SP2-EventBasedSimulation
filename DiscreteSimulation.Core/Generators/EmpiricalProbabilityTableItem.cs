namespace DiscreteSimulation.Core.Generators;

public class EmpiricalProbabilityTableItem
{
    public double LowerBound { get; set; }
    
    public double UpperBound { get; set; }
    
    public double Probability { get; set; }
    
    public EmpiricalProbabilityTableItem(double lowerBound, double upperBound, double probability)
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;
        Probability = probability;
    }
}
