var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\05-1\input.txt");

var blocks = SplitIntoBlocks(lines).ToList();
var seedGroups = GetSeeds(blocks.First().Single()).ToArray();
var mapsBySource = GetMapsFromBlocksOfLines(blocks.Skip(1))
    .ToDictionary(x => x.Source, x => x);

Console.WriteLine(GetMinLocation(seedGroups, mapsBySource));


long GetMinLocation(SeedGroup[] seedGroups, Dictionary<string, Map> mapsBySource)
{
    object locker = new object();
    long minLocation = long.MaxValue;

    Parallel.ForEach(seedGroups, new ParallelOptions { MaxDegreeOfParallelism = 8 }, group =>
    {
        Console.WriteLine($"Starting seed group {group.Start}-{group.Start + group.Length}");

        long seed = group.Start;
        while(seed < group.Start + group.Length)
        {
            Console.WriteLine($"Group {group.Start}-{group.Start + group.Length} checking seed {seed}");
            State state = new State(seed, "seed");

            while (state.Type != "location")
            {
                state = GetNextState(state, mapsBySource);
            }

            if (state.Type == "location")
            {
                if (state.Value < minLocation)
                {
                    lock (locker)
                    {
                        if (state.Value < minLocation)
                        {
                            minLocation = state.Value;
                        }
                    }
                }
            }

            Console.WriteLine($"Group {group.Start} skipping {state.NextUsefulIncrement} seeds");
            seed += state.NextUsefulIncrement == 1 ? 1 : state.NextUsefulIncrement - 1;
        }
    });

    return minLocation;
}

State GetNextState(State currentState, Dictionary<string, Map> dictionary)
{
    var map = dictionary[currentState.Type];

    var matchingMapEntry = map.Entries.Where(x =>
        currentState.Value >= x.SourceStart && currentState.Value <= x.SourceStart + x.Length).ToArray();
    long nextUsefulIncrement;
    if (matchingMapEntry.Any())
    {
        var matchingEntry = matchingMapEntry.First();
        currentState.Value += matchingEntry.TargetStart - matchingEntry.SourceStart;
        nextUsefulIncrement = matchingEntry.TargetStart + matchingEntry.Length - currentState.Value + 1;
    }
    else
    {
        nextUsefulIncrement = map.GetNextEntrySourceValueAfter(currentState.Value) - currentState.Value;
    }
    currentState.Type = map.Target;

    if (nextUsefulIncrement < currentState.NextUsefulIncrement)
    {
        currentState.NextUsefulIncrement = nextUsefulIncrement;
    }
    
    return currentState;
}

IEnumerable<SeedGroup> GetSeeds(string line)
{
    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    
    long? rangeStart = null;
    
    foreach (var part in parts)
    {
        if (long.TryParse(part, out var result))
        {
            if (rangeStart is null) { rangeStart = result; }
            else
            {
                yield return new SeedGroup(rangeStart.Value, result);
                rangeStart = null;
            }
        }
    }
}

IEnumerable<Map> GetMapsFromBlocksOfLines(IEnumerable<string[]> blocks)
{
    foreach (var block in blocks)
    {
        var headerParts = block[0].Split(new [] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        List<MapEntry> entries = new List<MapEntry>();
        for (int i = 1; i < block.Length; i++)
        {
            var entryParts = block[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var parsedEntryParts = entryParts.Select(x => long.Parse(x.Trim())).ToArray();
            entries.Add(new MapEntry(parsedEntryParts[0], parsedEntryParts[1], parsedEntryParts[2]));
        }
        
        yield return new Map(headerParts[0], headerParts[2], entries.ToArray());
    }
}

IEnumerable<string[]> SplitIntoBlocks(string[] lines)
{
    List<string> result = new List<string>();
    for (int i = 0; i < lines.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(lines[i]))
        {
            yield return result.ToArray();
            result.Clear();
        }
        else
        {
            result.Add(lines[i]);
        }
    }
    yield return result.ToArray();
}

struct State
{
    public State(long value, string type)
    {
        Value = value;
        Type = type;
    }
    
    public long Value { get; set; }
    public string Type { get; set; }
    public long NextUsefulIncrement { get; set; } = long.MaxValue;
}

record SeedGroup(long Start, long Length);

record Map(string Source, string Target, MapEntry[] Entries)
{
    public long GetNextEntrySourceValueAfter(long sourceValue)
    {
        var nextEntries = Entries
            .Select(e => e.SourceStart)
            .Where(i => i > sourceValue)
            .ToArray();
        return nextEntries.Any() ? nextEntries.Min() : long.MaxValue;
    }
}

record MapEntry (long TargetStart, long SourceStart,  long Length);