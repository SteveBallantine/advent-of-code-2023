var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\11-1\test-input1.txt");
var width = lines[0].Length;

Direction[] Directions =
{
    new ('N', 0, -1, 'S'),
    new ('S', 0, 1, 'N'),
    new ('E', 1, 0, 'W'),
    new ('W', -1, 0, 'E'),
};

Map map = new Map(Directions, lines.Select(s => s.ToCharArray()).ToArray());

var emptyRows = map.GetRowsWithAllLabel('.').ToArray();
var emptyCols = map.GetColumnsWithAllLabel('.').ToArray();

var expanded = new char[lines.Length + emptyRows.Length][];
var emptyLine = new char[lines[0].Length + emptyCols.Length];
for (int i = 0; i < emptyLine.Length; i++)
{
    emptyLine[i] = '.';
}

int expandedICount = 0;
for (int i = 0; i < expanded.Length; i++)
{
    var oldY = i - expandedICount;

    if (emptyRows.Contains(oldY))
    {
        expanded[i] = emptyLine;
        expanded[i + 1] = emptyLine;
        i++;
        expandedICount++;
    }
    else
    {
        expanded[i] = new char[lines[0].Length + emptyCols.Length];
        int expandedJCount = 0;
        for (int j = 0; j < expanded[i].Length; j++)
        {
            var oldX = j - expandedJCount;
            if (emptyCols.Contains(oldX))
            {
                expanded[i][j] = '.';
                expanded[i][j + 1] = '.';
                j++;
                expandedJCount++;
            }
            else
            {
                expanded[i][j] = lines[oldY][oldX];
            }
        }
    }
}

foreach (var line in expanded)
{
    Console.WriteLine(line);
}

var expandedMap = new Map(Directions, expanded);

var galaxies = expandedMap.FindLabel('#').ToArray();
Console.WriteLine($"{galaxies.Length} galaxies");

var allPairs = galaxies.SelectMany(a => galaxies
    .Where(b => a != b)
    .Select(b => new PointPair(a, b)))
    .ToArray();

var realPairs = new HashSet<PointPair>();
foreach (var pair in allPairs)
{
    var reversePair = new PointPair(pair.B, pair.A);
    if (!realPairs.Contains(reversePair))
    {
        realPairs.Add(pair);
    }
}
Console.WriteLine($"{realPairs.Count} pairs");

var distances = realPairs.Select(GetDistance);

Console.WriteLine(distances.Sum());


int GetDistance(PointPair pair)
{
    int dXn = Math.Abs(pair.A.X - pair.B.X);
    int dYn = Math.Abs(pair.A.Y - pair.B.Y);
    return dXn + dYn;
}

record Direction(char Label, int DeltaX, int DeltaY, char OppositeDirectionLabel);

class Map
{
    private readonly char[][] _locations;
    private readonly Dictionary<char, Direction> _directions;
    
    public Map(Direction[] directions, char[][] locations)
    {
        _directions = directions.ToDictionary(d => d.Label, d => d);
        _locations = locations;
        var width = locations[0].Length;
        if (locations.Any(x => x.Length != width))
        {
            throw new Exception("All lines do not have same width");
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
}

record PointPair(Point A, Point B);

record Point(int X, int Y);
