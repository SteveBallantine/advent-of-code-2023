using System.Diagnostics;

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\14-1\input.txt");

Direction[] Directions =
{
    new ('N', 0, -1, 'S'),
    new ('W', -1, 0, 'E'),
    new ('S', 0, 1, 'N'),
    new ('E', 1, 0, 'W'),
};

AssertFor(@"O....#....
O.OO#....#
.....##...
OO.#O....O
.O.....O#.
O.#..O.#.#
..O..#O..O
.......O..
#....###..
#OO..#....", 64);

Console.Write(RunFor(lines));


long RunFor(string[] input)
{
    var rows = input.Select(s => s.ToCharArray()).ToArray();

    Map map = new Map(rows);

    Dictionary<int, long> history = new Dictionary<int, long>();
    int currentState = map.GetHashCode();
    int c = 0;
    //map.LogToConsole();
    
    while (!history.ContainsKey(currentState))
    {
        c++;
        history.Add(currentState, GetLoad(map));

        foreach (var d in Directions)
        {
            map.TiltInDirection(d);
        }
        //Console.WriteLine(c);
        //map.LogToConsole();
        
        currentState = map.GetHashCode();
    }
    
    Console.WriteLine($"Loop after {history.Count} cycles");

    int leadIn = 0;
    foreach (var h in history)
    {
        if (h.Key == currentState)
        {
            break;
        } 
        leadIn++;
    }
    var cycleLength = history.Count - leadIn;
    
    Console.WriteLine($"Lead in {leadIn}. Cycle length {cycleLength}");
        
    var historyIndex = (1000000000 - leadIn) % cycleLength;
    
    var historyList = history.ToList();
    int x = 0;
    foreach (var entry in historyList)
    {
        Console.Write($" | {x} {entry.Value}");
        x++;
    }
    Console.WriteLine();
    Console.WriteLine($"Index = {historyIndex + leadIn}");
    
    Console.WriteLine($"Answer = {historyList[historyIndex + leadIn].Value}");
    return historyList[historyIndex + leadIn].Value;
}

long GetLoad(Map map)
{
    long total = 0;
    foreach (var col in map.GetColumns())
    {
        var colScore = 0l;
        for(int i = 0; i < col.Length; i++)
        {
            if (col[i] == 'O')
            {
                colScore += col.Length - i;
            }
        }
        total += colScore;
    }

    return total;
}

void AssertFor(string input, long expected)
{
    Debug.Assert(RunFor(input.Split(Environment.NewLine)) == expected);
}


record Direction(char Label, int DeltaX, int DeltaY, char OppositeDirectionLabel);

class Map
{
    private readonly char[][] _locations;

    public Map(char[][] locations)
    {
        _locations = locations;
        var width = locations[0].Length;
        if (locations.Any(x => x.Length != width))
        {
            throw new Exception("All lines do not have same width");
        }
    }

    public void LogToConsole()
    {
        foreach (var r in _locations)
        {
            Console.WriteLine(new string(r));
        }
    }
    
    public void TiltInDirection(Direction d)
    {
        char[][] data;
        bool reverse = d.DeltaY > 0;
        if (d.DeltaX != 0)
        {
            data = _locations;
            reverse = d.DeltaX > 0;
        }
        else if (d.DeltaY != 0)
        {
            data = GetColumns().ToArray();
            reverse = d.DeltaY > 0;
        }
        else
        {
            throw new Exception("Bad");
        }

        foreach (var line in data)
        {
            MoveRocks(line, reverse);
        }

        
        if (d.DeltaY != 0)
        {
            for(int y = 0; y < data.Length; y++)
            {
                for (int x = 0; x < data[y].Length; x++)
                {
                    _locations[y][x] = data[x][y];
                }
            }
        }
    }

    private void MoveRocks(char[] line, bool reverse)
    {
        var previousSpaces = 0;
        int i = reverse ? line.Length - 1 : 0;
        int increment = reverse ? -1 : 1;
        
        while((reverse && i >= 0) ||
              (!reverse && i < line.Length))
        {
            switch (line[i])
            {
                case '.':
                    previousSpaces++;
                    break;
                case '#':
                    previousSpaces = 0;
                    break;
                case 'O':
                    if (previousSpaces > 0)
                    {
                        line[i] = '.';
                        line[i - previousSpaces * increment] = 'O';
                    }
                    break;
                default:
                    throw new Exception("ARG!");
            }
            i += increment;
        }
    }
    
    public IEnumerable<char[]> GetColumns()
    {
        for (int c = 0; c < _locations[0].Length; c++)
        {
            char[] result = new char[_locations.Length];
            for (int r = 0; r < _locations.Length; r++)
            {
                result[r] = _locations[r][c];
            }
            yield return result;
        }
    }

    public IEnumerable<int> GetRowsWithAllLabel(char label)
    {
        for (var row = 0; row < _locations.Length; row++)
        {
            if (_locations[row].All(c => c == label))
            {
                yield return row;
            }
        }
    }
    
    public IEnumerable<int> GetColumnsWithAllLabel(char label)
    {
        for (var col = 0; col < _locations[0].Length; col++)
        {
            var allSame = true;
            foreach (var row in _locations)
            {
                allSame = allSame && row[col] == label;
            }

            if (allSame) yield return col;
        }
    }

    public IEnumerable<Point> FindLabel(char label)
    {
        for (var y = 0; y < _locations.Length; y++)
        {
            for (var x = 0; x < _locations[0].Length; x++)
            {
                if (_locations[y][x] == label)
                {
                    yield return new Point(x, y);
                }
            }
        }
    }
    
    public override int GetHashCode()
    {
        int x = 0;
        int count = 0;
        foreach (var entry in _locations)
        {
            foreach (var ch in entry)
            {
                count++;
                x ^= ch.GetHashCode() * count;
            }
        }
        return x;
    }
}

record PointPair(Point A, Point B);

record Point(int X, int Y);