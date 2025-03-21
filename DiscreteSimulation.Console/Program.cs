using DiscreteSimulation.Core.Generators;

var seedGenerator = new SeedGenerator();

/*
var empiricalProbabilityGenerator = new EmpiricalProbabilityGenerator(isDiscrete: true, [
    new EmpiricalProbabilityTableItem(30, 60, 0.2),
    new EmpiricalProbabilityTableItem(60, 100, 0.4),
    new EmpiricalProbabilityTableItem(100, 140, 0.3),
    new EmpiricalProbabilityTableItem(140, 160, 0.1)
], seedGenerator);

var countResults = new int[4];
var numberOfIterations = 10_000_000;

for (var i = 0; i < numberOfIterations; i++)
{
    Console.WriteLine(i);
    
    var next = empiricalProbabilityGenerator.NextInt();
    
    if (next >= 30 && next < 60)
    {
        countResults[0]++;
    }
    else if (next >= 60 && next < 100)
    {
        countResults[1]++;
    }
    else if (next >= 100 && next < 140)
    {
        countResults[2]++;
    }
    else if (next >= 140 && next < 160)
    {
        countResults[3]++;
    }
}

Console.WriteLine("<30, 60): " + countResults[0] / (double)numberOfIterations);
Console.WriteLine("<60, 100): " + countResults[1] / (double)numberOfIterations);
Console.WriteLine("<100, 140): " + countResults[2] / (double)numberOfIterations);
Console.WriteLine("<140, 160): " + countResults[3] / (double)numberOfIterations);
*/

var empiricalProbabilityGenerator = new EmpiricalProbabilityGenerator(isDiscrete: false, [
    new EmpiricalProbabilityTableItem(5, 10, 0.2),
    new EmpiricalProbabilityTableItem(10, 50, 0.4),
    new EmpiricalProbabilityTableItem(50, 70, 0.3),
    new EmpiricalProbabilityTableItem(70, 80, 0.06),
    new EmpiricalProbabilityTableItem(80, 95, 0.04)
], seedGenerator);

var countResults = new int[5];
var numberOfIterations = 10_000_000;

for (var i = 0; i < numberOfIterations; i++)
{
    Console.WriteLine(i);
    
    var next = empiricalProbabilityGenerator.Next();
    
    if (next >= 5 && next < 10)
    {
        countResults[0]++;
    }
    else if (next >= 10 && next < 50)
    {
        countResults[1]++;
    }
    else if (next >= 50 && next < 70)
    {
        countResults[2]++;
    }
    else if (next >= 70 && next < 80)
    {
        countResults[3]++;
    }
    else if (next >= 80 && next < 95)
    {
        countResults[4]++;
    }
}

Console.WriteLine("<5, 10): " + countResults[0] / (double)numberOfIterations);
Console.WriteLine("<10, 50): " + countResults[1] / (double)numberOfIterations);
Console.WriteLine("<50, 70): " + countResults[2] / (double)numberOfIterations);
Console.WriteLine("<70, 80): " + countResults[3] / (double)numberOfIterations);
Console.WriteLine("<80, 95): " + countResults[4] / (double)numberOfIterations);

