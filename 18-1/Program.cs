using System.Linq.Expressions;

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

AssertFor(@"R 6 (#70c710)
D 5 (#0dc571)
L 2 (#5713f0)
D 2 (#d2c081)
R 2 (#59c680)
D 2 (#411b91)
L 5 (#8ceee2)
U 2 (#caa173)
L 1 (#1b58a2)
U 2 (#caa171)
R 2 (#7807d2)
U 3 (#a77fa3)
L 2 (#015232)
U 2 (#7a21e3)", 62);

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\18-1\input.txt");
Console.WriteLine(RunFor(lines, true));

long RunFor(string[] input, bool logging)
{
    MapLocation[][] startData = new MapLocation[400][];
    for(int i = 0; i < 400; i++)
    {
        startData[i] = new MapLocation[500];
        for (int j = 0; j < 500; j++)
        {
            startData[i][j] = new MapLocation { Label = '.' };
        }
    }
    Map<MapLocation> map = new Map<MapLocation>(directions, startData);

    var count = DigTrench(input, map);
    count += DigInterior(map, 251, 251);
    
    return count;
}

int DigInterior(Map<MapLocation> map, int x, int y)
{
    var result = 0;
    var nextPoints = new Stack<Point>();
    nextPoints.Push(new Point(x, y));

    while (nextPoints.Count > 0)
    {
        var thisPoint =  nextPoints.Pop();
        if (map.PointIsInBounds(thisPoint) &&
            map.GetState(thisPoint.X, thisPoint.Y).Label == '.')
        {
            map.UpdateState(thisPoint.X, thisPoint.Y, l => l.Label = '#');
            result++;
            foreach (var direction in directions)
            {
                nextPoints.Push(new Point(thisPoint.X + direction.DeltaX, thisPoint.Y + direction.DeltaY));
            }
        }
    }

    return result;
}


int DigTrench(string[] strings, Map<MapLocation> map1)
{
    int x = 250;
    int y = 250;
    int count = 0;

    foreach (var line in strings)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Direction direction;
        switch (parts[0])
        {
            case "U":
                direction = n;
                break;
            case "D":
                direction = s;
                break;
            case "L":
                direction = w;
                break;
            case "R":
                direction = e;
                break;
            default:
                throw new Exception("Bad label");
        }

        int length = int.Parse(parts[1]);
        map1.UpdateState(x, y, l => { l.Label = '#'; });

        for (int i = 0; i < length; i++)
        {
            x += direction.DeltaX;
            y += direction.DeltaY;

            map1.UpdateState(x, y, l =>
            {
                l.Label = '#';
                l.HexColour = parts[2];
            });
            count++;
        }
    }

    return count;
}

void AssertFor(string input, long expectedResult)
{
    var lines = input.Split(System.Environment.NewLine);
    var result = RunFor(lines, false);
    if (result != expectedResult)
    {
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
        throw new Exception($"Result was {result} but expected {expectedResult}");
    }
}


record MapLocation() 
{
    public char Label { get; set; }
    public string? HexColour { get; set; }
};

record Direction(char Label, int DeltaX, int DeltaY, char OppositeDirectionLabel);

class Map<T>
{
    private readonly T[][] _locations;
    private readonly Dictionary<char, Direction> _directions;
    
    public int Width => _locations[0].Length;
    public int Height => _locations.Length;
    
    public Map(Direction[] directions, T[][] locations)
    {
        _directions = directions.ToDictionary(d => d.Label, d => d);
        _locations = locations;
        var width = locations[0].Length;
        if (locations.Any(x => x.Length != width))
        {
            throw new Exception("All lines do not have same width");
        }
    }

    public T GetState(int x, int y)
    {
        return _locations[y][x];
    }
    public void UpdateState(int x, int y, Action<T> update)
    {
        update(_locations[y][x]);
    }

    public IEnumerable<Point> FindAll(Func<T, bool> criteria)
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (criteria(_locations[y][x]))
                {
                    yield return new Point(x, y);
                }
            }
        }
    }

    private PointPair? GetNextPointInDirection(Direction d, Point p)
    {
        var nextPoint = new Point(p.X + d.DeltaX, p.Y + d.DeltaY);
        return PointIsInBounds(nextPoint) ? new PointPair(p, nextPoint, d) : null;
    }
    
    public bool PointIsInBounds(Point p)
    {
        return p.X >= 0 && p.X < Width &&
               p.Y >= 0 && p.Y < Height;
    }
    
    private IEnumerable<PointPair> GetAdjacentPoints(Point p)
    {
        return _directions.Select(d => GetNextPointInDirection(d.Value, p)).Where(x => x is not null);
    }
}

record PointPair(Point A, Point B, Direction DirectionFromAToB);

record Point(int X, int Y);
