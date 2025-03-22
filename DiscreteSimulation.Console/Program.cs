using DiscreteSimulation.Core.Generators;

var seedGenerator = new SeedGenerator();

var triangularDistributionGenerator = new TriangularDistributionGenerator(60, 480, 120, seedGenerator.Next());

using var triangularDistributionFile = new StreamWriter("triangular_distribution.txt");

for (var i = 0; i < 50_000; i++)
{
    var next = triangularDistributionGenerator.Next();
    triangularDistributionFile.WriteLine(next.ToString().Replace(",", "."));
}

var exponentialDistributionGenerator = new ExponentialDistributionGenerator(1/30.0, seedGenerator.Next());

using var exponentialDistributionFile = new StreamWriter("exponential_distribution.txt");

for (var i = 0; i < 50_000; i++)
{
    var exponentialDistribution = exponentialDistributionGenerator.Next();
    exponentialDistributionFile.WriteLine(exponentialDistribution.ToString().Replace(",", "."));
}
