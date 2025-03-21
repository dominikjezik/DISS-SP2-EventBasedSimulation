namespace DiscreteSimulation.Core.Generators;

public class EmpiricalProbabilityGenerator
{
    private readonly bool _isDiscrete;

    private readonly List<EmpiricalProbabilityTableItem> _probabilityTable;

    private readonly Random[] _randoms;

    public EmpiricalProbabilityGenerator(bool isDiscrete, List<EmpiricalProbabilityTableItem> probabilityTable, List<int> seedsForGenerators)
    {
        _isDiscrete = isDiscrete;
        _probabilityTable = probabilityTable;
        
        if (seedsForGenerators.Count != probabilityTable.Count + 1)
        {
            throw new ArgumentException("Number of seeds in list of seeds must be equal to number of items in probability table + 1");
        }
        
        _randoms = new Random[probabilityTable.Count + 1];
        
        SetupGenerators(seedsForGenerators);
    }
    
    public EmpiricalProbabilityGenerator(bool isDiscrete, List<EmpiricalProbabilityTableItem> probabilityTable, SeedGenerator seedGenerator)
    {
        var listOfSeeds = new List<int>(probabilityTable.Count + 1);
        
        for (var i = 0; i < probabilityTable.Count + 1; i++)
        {
            listOfSeeds.Add(seedGenerator.Next());
        }
        
        _isDiscrete = isDiscrete;
        _probabilityTable = probabilityTable;
        
        _randoms = new Random[probabilityTable.Count + 1];
        
        SetupGenerators(listOfSeeds);
    }

    private void SetupGenerators(List<int> seedsForGenerators)
    {
        _randoms[0] = new Random(seedsForGenerators[0]);
        
        var probabilitySumCheck = 0.0;

        for (var i = 0; i < _probabilityTable.Count; i++)
        {
            probabilitySumCheck += _probabilityTable[i].Probability;
            _randoms[i + 1] = new Random(seedsForGenerators[i + 1]);
        }
        
        var difference = Math.Abs(probabilitySumCheck - 1.0);
        
        if (difference > 0.00000001)
        {
            throw new ArgumentException("Sum of all probabilities in table must by 1");
        }
    }
    
    public double Next()
    {
        if (_isDiscrete)
        {
            throw new InvalidOperationException("This method can be called only for continuous generator");
        }
        
        var probabilityForSelectingTableItem = _randoms[0].NextDouble();

        var cumulativeProbabilities = 0.0;
        
        for (var i = 0; i < _probabilityTable.Count; i++)
        {
            cumulativeProbabilities += _probabilityTable[i].Probability;
            
            if (probabilityForSelectingTableItem < cumulativeProbabilities)
            {
                return NextFromTableItem(i);
            }
        }
        
        throw new Exception("This exception should never be thrown");
    }

    public int NextInt()
    {
        if (!_isDiscrete)
        {
            throw new InvalidOperationException("This method can be called only for discrete generator");
        }
        
        var probabilityForSelectingTableItem = _randoms[0].NextDouble();

        var cumulativeProbabilities = 0.0;
        
        for (var i = 0; i < _probabilityTable.Count; i++)
        {
            cumulativeProbabilities += _probabilityTable[i].Probability;
            
            if (probabilityForSelectingTableItem < cumulativeProbabilities)
            {
                return (int)NextFromTableItem(i);
            }
        }
        
        throw new Exception("This exception should never be thrown");
    }
    
    private double NextFromTableItem(int tableItemIndex)
    {
        var random = _randoms[tableItemIndex + 1];
        var intervalWidth = _probabilityTable[tableItemIndex].UpperBound - _probabilityTable[tableItemIndex].LowerBound;

        if (!_isDiscrete)
        {
            return random.NextDouble() * intervalWidth + _probabilityTable[tableItemIndex].LowerBound;
        }
        
        var lowerBound = (int)_probabilityTable[tableItemIndex].LowerBound;
        var upperBound = (int)_probabilityTable[tableItemIndex].UpperBound;

        return random.Next(lowerBound, upperBound);
    }
}
