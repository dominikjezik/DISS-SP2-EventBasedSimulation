using DiscreteSimulation.Console;
using DiscreteSimulation.Core.Generators;

//var analyzer = new ConfigurationAnalyzer(@"C:\Users\Dominik\Desktop\");
var analyzer = new ConfigurationAnalyzer("/Users/dominik/Desktop/");

analyzer.Configurations = new[,]
{
    // 3.2 Séria experimentov s konfiguráciami <a,1,1> pre a=1,2,3,4
    /*
    { 1, 1, 1 },
    { 2, 1, 1 },
    { 3, 1, 1 },
    { 4, 1, 1 }
    */
    
    // 3.3 Séria experimentov s konfiguráciami <2,1,c> pre c=1,..,20
    /*
    { 2, 1, 1 },
    { 2, 1, 2 },
    { 2, 1, 3 },
    { 2, 1, 4 },
    { 2, 1, 5 },
    { 2, 1, 6 },
    { 2, 1, 7 },
    { 2, 1, 8 },
    { 2, 1, 9 },
    { 2, 1, 10 },
    { 2, 1, 11 },
    { 2, 1, 12 },
    { 2, 1, 13 },
    { 2, 1, 14 },
    { 2, 1, 15 },
    { 2, 1, 16 },
    { 2, 1, 17 },
    { 2, 1, 18 },
    { 2, 1, 19 },
    { 2, 1, 20 }
    */
    
    // 3.4 Séria experimentov s konfiguráciami <2,b,18> pre b=1,..,5
    /*
    { 2, 1, 18 },
    { 2, 2, 18 },
    { 2, 3, 18 },
    { 2, 4, 18 },
    { 2, 5, 18 }
    */
    
    
    
    // 3.5 Okolie experimentu s konfiguráciou <2,2,18>
    { 2, 2, 18 },
    
    { 1, 2, 18 },
    { 3, 2, 18 },
    { 2, 1, 18 },
    { 2, 3, 18 },
    { 2, 2, 17 },
    { 2, 2, 19 },
    
    { 1, 1, 18 },
    { 1, 3, 18 },
    { 3, 1, 18 },
    { 3, 3, 18 },
    { 1, 2, 17 },
    { 1, 2, 19 },
    { 3, 2, 17 },
    { 3, 2, 19 },
    { 2, 1, 17 },
    { 2, 1, 19 },
    { 2, 3, 17 },
    { 2, 3, 19 },
    
    { 1, 1, 17 },
    { 1, 1, 19 },
    { 1, 3, 17 },
    { 1, 3, 19 },
    { 3, 1, 17 },
    { 3, 1, 19 },
    { 3, 3, 17 },
    { 3, 3, 19 }
    
};

analyzer.AnalyzeConfigurations(1_000);







/*
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
*/
