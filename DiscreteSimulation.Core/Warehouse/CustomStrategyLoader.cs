namespace DiscreteSimulation.Core.Warehouse;

public class CustomStrategyLoader
{
    public int[,] LoadStrategyFromFile(Uri pathToFile)
    {
        var strategies = new int[30,6];
        
        using var reader = new StreamReader(pathToFile.LocalPath);
        
        for (int i = 0; i < 30; i++)
        {
            var line = reader.ReadLine();
            var numbers = line.Split(' ');
            
            for (int j = 0; j < 6; j++)
            {
                strategies[i, j] = int.Parse(numbers[j]);
            }
        }
        
        return strategies;
    }
}
