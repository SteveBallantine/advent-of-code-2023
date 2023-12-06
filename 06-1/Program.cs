var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\06-1\input.txt");

var races = GetRaces(lines).Select(PopulateRaceWithMinMaxValues);
var waysToWin = races.Select(r => r.MaxButtonPressTimeToWin - r.MinButtonPressTimeToWin + 1).ToArray();
long result = waysToWin.First().Value;
foreach (var entry in waysToWin.Skip(1))
{
    result *= entry.Value;
}

Console.WriteLine(result);


Race PopulateRaceWithMinMaxValues(Race race)
{
    race.MinButtonPressTimeToWin = FindMinWinningButtonTime(race);
    race.MaxButtonPressTimeToWin = FindMaxWinningButtonTime(race, race.MinButtonPressTimeToWin.Value);
    Console.WriteLine($"Race time {race.Time}. Dist to win {race.MinDistanceToWin}. Wining press times {race.MinButtonPressTimeToWin} to {race.MaxButtonPressTimeToWin}");
    Console.WriteLine($"{race.MinButtonPressTimeToWin - 1} = {GetDistanceForButtonTime(race, race.MinButtonPressTimeToWin.Value - 1)}");
    Console.WriteLine($"{race.MinButtonPressTimeToWin} = {GetDistanceForButtonTime(race, race.MinButtonPressTimeToWin.Value)}");
    Console.WriteLine($"{race.MinButtonPressTimeToWin + 1} = {GetDistanceForButtonTime(race, race.MinButtonPressTimeToWin.Value + 1)}");
    Console.WriteLine("-");
    Console.WriteLine($"{race.MaxButtonPressTimeToWin - 1} = {GetDistanceForButtonTime(race, race.MaxButtonPressTimeToWin.Value - 1)}");
    Console.WriteLine($"{race.MaxButtonPressTimeToWin} = {GetDistanceForButtonTime(race, race.MaxButtonPressTimeToWin.Value)}");
    Console.WriteLine($"{race.MaxButtonPressTimeToWin + 1} = {GetDistanceForButtonTime(race, race.MaxButtonPressTimeToWin.Value + 1)}");
    Console.WriteLine("==============================================");
    return race;
}

int FindMinWinningButtonTime(Race race)
{
    var min = 0;
    var max = race.Time;
    
    while (min < max)
    {
        int mid = (min + max) / 2;
        var distance = GetDistanceForButtonTime(race, mid);
        var distanceRising = distance < GetDistanceForButtonTime(race, mid + 1);
        if (distanceRising)
        {
            if (distance > race.MinDistanceToWin)
            {
                max = mid;
            }
            if (distance <= race.MinDistanceToWin)
            {
                min = mid + 1;
            }
        }
        else
        {
            max = mid - 1;
        }
    }
    return min;
}

int FindMaxWinningButtonTime(Race race, int minButtonWinningTime)
{
    var min = minButtonWinningTime;
    var max = race.Time;
    
    while (min < max)
    {
        int mid = (min + max) / 2;
        if (mid == min) mid = max;
        var distance = GetDistanceForButtonTime(race, mid);
        var distanceRising = distance < GetDistanceForButtonTime(race, mid + 1);
        if (distanceRising)
        {
            min = mid + 1;
        }
        else
        {
            if (distance > race.MinDistanceToWin)
            {
                min = mid;
            }
            if (distance <= race.MinDistanceToWin)
            {
                max = mid - 1;
            }
        }
    }
    return min;
}

int GetDistanceForButtonTime(Race race, int timeToPress)
{
    var speed = timeToPress;
    var remainingTime = race.Time - timeToPress;
    return remainingTime * speed;
}

IEnumerable<Race> GetRaces(string[] lines)
{
    if (lines.Length != 2)
    {
        throw new Exception("Wrong number of lines");
    }
    
    var timeStrings = lines[0].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    var minDistanceStrings = lines[1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

    if (timeStrings.Length != minDistanceStrings.Length)
    {
        throw new Exception("Time/Distance mismatch");
    }
    
    for (int i = 1; i < timeStrings.Length; i++)
    {
        if (!int.TryParse(timeStrings[i], out var time))
        {
            throw new Exception($"Bad time {timeStrings[i]}");
        }
        if (!int.TryParse(minDistanceStrings[i], out var minDistance))
        {
            throw new Exception($"Bad time {minDistanceStrings[i]}");
        }

        yield return new Race(time, minDistance);
    }
}



record Race(int Time, int MinDistanceToWin)
{
    public int? MinButtonPressTimeToWin { get; set; }
    public int? MaxButtonPressTimeToWin { get; set; }
}
    
