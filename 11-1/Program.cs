var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\11-1\input.txt");
Map map = new Map(lines.Select(s => s.ToCharArray()).ToArray());

var emptyRows = map.GetRowsWithAllLabel('.').ToArray();
var emptyCols = map.GetColumnsWithAllLabel('.').ToArray();

var galaxies = map.FindLabel('#').ToArray();
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


long GetDistance(PointPair pair)
{
    int dXn = Math.Abs(pair.A.X - pair.B.X);
    int dYn = Math.Abs(pair.A.Y - pair.B.Y);

    var largerX = pair.A.X > pair.B.X ? pair.A.X : pair.B.X;
    var smallerX = pair.A.X < pair.B.X ? pair.A.X : pair.B.X;
    var largerY = pair.A.Y > pair.B.Y ? pair.A.Y : pair.B.Y;
    var smallerY = pair.A.Y < pair.B.Y ? pair.A.Y : pair.B.Y;

    var expandedColCrossings = Enumerable.Range(smallerX, largerX - smallerX).Count(n => emptyCols.Contains(n));
    var expandedRowCrossings = Enumerable.Range(smallerY, largerY - smallerY).Count(n => emptyRows.Contains(n));
    var multiplier = 999999;

    long ans = dXn + dYn + (expandedColCrossings + expandedRowCrossings) * multiplier;
    //Console.WriteLine($"{pair.A.X},{pair.A.Y} - {pair.B.X},{pair.B.Y} x {expandedColCrossings},{expandedRowCrossings} = {ans}");
    
    return ans;
}

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
