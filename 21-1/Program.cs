Direction[] directions =
{
    new('N', 0, -1, 'S'),
    new('S', 0, 1, 'N'),
    new('E', 1, 0, 'W'),
    new('W', -1, 0, 'E'),
};
var n = directions.Single(d => d.Label == 'N');
var s = directions.Single(d => d.Label == 'S');
var e = directions.Single(d => d.Label == 'E');
var w = directions.Single(d => d.Label == 'W');

var exampleInput = @"...........
.....###.#.
.###.##..#.
..#.#...#..
....#.#....
.##..S####.
.##..#...#.
.......##..
.##.#.####.
.##..##.##.
...........";
AssertFor(exampleInput, 6, false, 16);

Console.WriteLine(RunFor(File.ReadAllLines(@"C:\Repos\advent-of-code-2023\21-1\input.txt"), 64, false, true));



long RunFor(string[] input, int stepsToTake, bool part2, bool logging)
{
    var (map, start) = ParseInput(input);
    
    var currentPoints = new HashSet<Point>() { start };
    
    for (int i = 0; i < stepsToTake; i++)
    {
        var nextStepPoints = new HashSet<Point>();
        foreach (var point in currentPoints)
        {
            foreach (var nextPoint in map.GetAdjacentPoints(point))
            {
                nextStepPoints.Add(nextPoint.B);
            }
        }
        currentPoints = nextStepPoints;
    }
    
    return currentPoints.Count;
}

void AssertFor(string input, int stepsToTake, bool part2, long expectedResult)
{
    var lines = input.Split(System.Environment.NewLine);
    var result = RunFor(lines, stepsToTake, part2, false);
    if (result != expectedResult)
    {
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
        throw new Exception($"Result was {result} but expected {expectedResult}");
    }
}

(Map Map, Point Start) ParseInput(string[] input)
{
    var passable = input.Select(s => s.Select(c => c is '.' or 'S').ToArray()).ToArray();
    var map = new Map(directions, passable);

    Point start = new Point(0, 0);
    for (int y = 0; y < input.Length; y++)
    {
        for (int x = 0; x < input[0].Length; x++)
        {
            if (input[y][x] == 'S')
            {
                start = new Point(x, y);
            }
        }
    }

    return (map, start);
}

record Direction(char Label, int DeltaX, int DeltaY, char OppositeDirectionLabel);

class Map
{
    private readonly bool[][] _locations;
    private readonly Dictionary<char, Direction> _directions;
    
    public int Width => _locations[0].Length;
    public int Height => _locations.Length;
    
    public Map(Direction[] directions, bool[][] locations)
    {
        _directions = directions.ToDictionary(d => d.Label, d => d);
        _locations = locations;
        var width = locations[0].Length;
        if (locations.Any(x => x.Length != width))
        {
            throw new Exception("All lines do not have same width");
        }
    }

    public bool IsPassable(Point p)
    {
        return _locations[p.Y][p.X];
    }

    private PointPair? GetNextPointInDirection(Direction d, Point p)
    {
        var nextPoint = new Point(p.X + d.DeltaX, p.Y + d.DeltaY);
        return PointIsInBounds(nextPoint) ? new PointPair(p, nextPoint, d) : null;
    }
    
    private bool PointIsInBounds(Point p)
    {
        return p.X >= 0 && p.X < Width &&
               p.Y >= 0 && p.Y < Height;
    }
    
    public IEnumerable<PointPair> GetAdjacentPoints(Point p)
    {
        var points = _directions.Select(d => GetNextPointInDirection(d.Value, p));
        return points.Where(x => x is not null && IsPassable(x.B));
    }
}

record PointPair(Point A, Point B, Direction DirectionFromAToB);

record Point(int X, int Y);
