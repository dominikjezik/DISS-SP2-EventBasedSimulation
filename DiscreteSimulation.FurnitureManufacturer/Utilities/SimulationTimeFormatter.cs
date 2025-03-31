namespace DiscreteSimulation.FurnitureManufacturer.Utilities;

public static class SimulationTimeFormatter
{
    public static string FormatToSimulationTime(this string simulationTime, bool shortFormat = false, bool timeOnly = false)
    {
        if (double.TryParse(simulationTime, out double time))
        {
            return FormatToSimulationTime(time, shortFormat, timeOnly);
        }
        
        return simulationTime;
    }
    
    public static string FormatToSimulationTime(this double simulationTime, bool shortFormat = false, bool timeOnly = false)
    {
        var workingDays = Math.Floor(simulationTime / 28_800);
        simulationTime -= workingDays * 28_800;
        
        var hours = Math.Floor(simulationTime / 3_600);
        simulationTime -= hours * 3_600;
        
        var minutes = Math.Floor(simulationTime / 60);
        simulationTime -= minutes * 60;
        
        var seconds = Math.Floor(simulationTime);
        simulationTime -= seconds;
        
        if (timeOnly)
        {
            hours += workingDays * 24;
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
        
        var week = Math.Floor(workingDays / 5);
        workingDays -= week * 5;
        
        string dayOfWeek = string.Empty;
        
        if (shortFormat)
        {
            switch (workingDays)
            {
                case 0:
                    dayOfWeek = "Mo";
                    break;
                case 1:
                    dayOfWeek = "Tu";
                    break;
                case 2:
                    dayOfWeek = "We";
                    break;
                case 3:
                    dayOfWeek = "Th";
                    break;
                case 4:
                    dayOfWeek = "Fr";
                    break;
            }
        }
        else
        {
            switch (workingDays)
            {
                case 0:
                    dayOfWeek = "Monday";
                    break;
                case 1:
                    dayOfWeek = "Tuesday";
                    break;
                case 2:
                    dayOfWeek = "Wednesday";
                    break;
                case 3:
                    dayOfWeek = "Thursday";
                    break;
                case 4:
                    dayOfWeek = "Friday";
                    break;
            }
        }
        
        hours += 6;

        if (shortFormat)
        {
            return $"W{week + 1}-{dayOfWeek} {hours:00}:{minutes:00}";
        }

        return $"[Week {week + 1} - {dayOfWeek}] {hours:00}:{minutes:00}:{seconds:00}";
    }
}
