var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\09-1\input.txt");

var nextValues = lines.Select(GetSequence).Select(s => s.CalculateNextValue());
Console.WriteLine(nextValues.Sum());


Sequence GetSequence(string line)
{
    var entries = line.Split(" ");
    return new Sequence(entries.Select(x => long.Parse(x)).ToArray());
}

class Sequence
{
    private long[] Values { get; set; }

    public Sequence(long[] values)
    {
        Values = values;
    }

    public long CalculateNextValue()
    {
        var nextSequence = GetNextSequence();
        long nextValue = 0;
        if (nextSequence.Values.Any(x => x != 0))
        {
            nextValue = nextSequence.CalculateNextValue();
        }
        return Values[^1] + nextValue;
    }

    private Sequence GetNextSequence()
    {
        return new Sequence(GetDifferences().ToArray());
    }

    private IEnumerable<long> GetDifferences()
    {
        long? previous = null;
        foreach (var entry in Values)
        {
            if (previous.HasValue)
            {
                yield return entry - previous.Value;
            }
            previous = entry;
        }
    }
}