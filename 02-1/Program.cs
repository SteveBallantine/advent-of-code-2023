var text = File.ReadAllText(@"C:\Repos\advent-of-code-2023\02-1\input-isobel.txt");

var games = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
    .Select(GetResultFromLine)
    .ToArray();

foreach (var game in games)
{
    Console.WriteLine($"Game {game.GameNumber} - {(game.IsValid ? "VALID" : "INVALID")}");
}

Console.WriteLine(games.Where(g => g.IsValid).Sum(x => x.GameNumber));

var minimumCubeCounts = games.Select(g => g.GetMinimumCubeCounts()).ToArray();
Console.WriteLine(minimumCubeCounts.Select(c => c.Power).Sum());


GameResult GetResultFromLine(string line)
{
    var parts = line.Trim().Split(':', StringSplitOptions.RemoveEmptyEntries);
    if(parts.Length != 2) { throw new Exception($"Too many ':' in {line}"); }

    var gameNumString = new string(parts[0].Where(char.IsDigit).ToArray());
    if(!int.TryParse(gameNumString, out var nameNum)) { throw new Exception($"Cannot parse game num in {gameNumString}"); }

    return new GameResult(nameNum, GetDrawData(parts[1]).ToArray());
}

IEnumerable<DrawData> GetDrawData(string serialisedData) 
{
    foreach(var drawInfo in serialisedData.Split(';', StringSplitOptions.RemoveEmptyEntries))
    {
        yield return new DrawData(GetDrawEntries(drawInfo).ToArray());
    }
}

IEnumerable<CubeDrawEntry> GetDrawEntries(string serialisedData)
{
    foreach (var cubeInfo in serialisedData.Split(','))
    {
        var parts = cubeInfo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new Exception($"More than two parts in {serialisedData}");
        }

        if (!int.TryParse(parts[0], out int count))
        {
            throw new Exception($"Unable to parse numeric part of {serialisedData}");
        }
        
        yield return new CubeDrawEntry(parts[1], count);
    }
}


record GameResult(int GameNumber, DrawData[] DrawData)
{
    public bool IsValid => DrawData.All(x => x.IsValid);

    public DrawData GetMinimumCubeCounts()
    {
        var drawEntries = DrawData.SelectMany(d => d.CubeDraws)
            .GroupBy(d => d.Colour)
            .Select(g => new CubeDrawEntry(g.Key, g.Max(x => x.Count)))
            .ToArray();
        return new DrawData(drawEntries);
    }
}

record DrawData(CubeDrawEntry[] CubeDraws)
{
    public bool IsValid => CubeDraws.All(x => x.IsValid);

    public int Power
    {
        get
        {
            if (CubeDraws.Length > 3)
            {
                throw new Exception("Too many cube colours");
            }

            var result = CubeDraws[0].Count;
            foreach (var entry in CubeDraws.Skip(1))
            {
                result *= entry.Count;
            }
            return result;
        }
    }
}

record CubeDrawEntry(string Colour, int Count)
{
    public bool IsValid =>
        Colour == "red" && Count <= 12 ||
        Colour == "green" && Count <= 13 ||
        Colour == "blue" && Count <= 14;
};