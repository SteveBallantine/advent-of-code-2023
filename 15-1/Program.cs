AssertFor(new [] { @"rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7" }, 145);

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\15-1\input.txt");
Console.WriteLine(RunFor(lines, true));


long RunFor(string[] input, bool logging)
{
    long result = 0;
    var segments = input[0].Split(',');
    var boxes = new Box[256];
    for (int i = 0; i < 256; i++)
    {
        boxes[i] = new Box(new Dictionary<string, Lens>());
    }
    var splitOn = new char[] { '=', '-' };

    foreach (var segment in segments)
    {
        var parts = segment.Split(splitOn);
        var label = parts[0];
        var index = GetHash(label);
        var remove = segment.Contains('-');

        var box = boxes[index];
        if (remove)
        {
            if (box.Lenses.ContainsKey(label))
            {
                foreach (var entry in box.Lenses.Where(k => k.Value.Order > box.Lenses[label].Order))
                {
                    entry.Value.Order--;
                }
                box.Lenses.Remove(label);
            }
        }
        else
        {
            var focalLength = int.Parse(parts[1]);
            if (box.Lenses.TryGetValue(label, out var lens))
            {
                lens.FocalLength = focalLength;
            }
            else
            {
                box.Lenses.Add(label, new Lens(label) { FocalLength = focalLength, Order = box.Lenses.Count } );
            }
        }

        /*if (logging)
        {
            LogBoxes(boxes);
            Console.WriteLine();
        }*/
    }

    int x = 0;
    foreach (var box in boxes)
    {
        x++;
        int l = 0;
        foreach (var lens in box.Lenses.Select(b => b.Value).OrderBy(b => b.Order))
        {
            l++;
            var thisBox = x * l * lens.FocalLength;
            result += thisBox;
            /*if (logging)
            {
                Console.WriteLine(thisBox);
            }*/
        }
    }
    
    return result;
}

void LogBoxes(Box[] boxes)
{
    int x = 0;
    foreach (var box in boxes)
    {
        if (box.Lenses.Count > 0)
        {
            Console.Write($"Box {x}");
        }
        foreach (var lens in box.Lenses)
        {
            Console.Write($"[{lens.Key} {lens.Value}]");
        }
        if (box.Lenses.Count > 0)
        {
            Console.WriteLine($"");
        }
    }
}

int GetHash(string segment)
{
    int result = 0;
    foreach (var x in segment)
    {
        result += x;
        result *= 17;
        result %= 256;
    }
    return result;
}

void AssertFor(string[] input, long expectedResult)
{
    var result = RunFor(input, false);
    if (result != expectedResult)
    {
        foreach (var line in input)
        {
            Console.WriteLine(line);
        }
        throw new Exception($"Result was {result} but expected {expectedResult}");
    }
}

record Box(Dictionary<string, Lens> Lenses);

record Lens(string Label)
{
    public int FocalLength { get; set; }
    public int Order { get; set; }
}