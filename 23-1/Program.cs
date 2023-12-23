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

Dictionary<char, Direction[]> validDirectionsFrom = new Dictionary<char, Direction[]>
{
    { '.', new Direction[] { n, s, e, w } },
    { '>', new Direction[] { e } },
    { '<', new Direction[] { w } },
    { '^', new Direction[] { n } },
    { 'v', new Direction[] { s } },
};
Dictionary<char, Direction[]> validDirectionsTo = new Dictionary<char, Direction[]>
{
    { '.', new Direction[] { n, s, e, w } },
    { '>', new Direction[] { n, s, e } },
    { '<', new Direction[] { n, s, w } },
    { '^', new Direction[] { e, w, n } },
    { 'v', new Direction[] { e, w, s } },
};


var exampleInput = @"#.#####################
#.......#########...###
#######.#########.#.###
###.....#.>.>.###.#.###
###v#####.#v#.###.#.###
###.>...#.#.#.....#...#
###v###.#.#.#########.#
###...#.#.#.......#...#
#####.#.#.#######.#.###
#.....#.#.#.......#...#
#.#####.#.#.#########v#
#.#...#...#...###...>.#
#.#.#v#######v###.###v#
#...#.>.#...>.>.#.###.#
#####v#.#.###v#.#.###.#
#.....#...#...#.#.#...#
#.#########.###.#.#.###
#...###...#...#...#.###
###.###.#.###v#####v###
#...#...#.#.>.>.#.>.###
#.###.###.#.###.#.#v###
#.....###...###...#...#
#####################.#";

AssertFor(exampleInput, 94);

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\23-1\input.txt");
Console.WriteLine(RunFor(lines, true));

long RunFor(string[] input, bool logging)
{
    var map = CreateMap(input);
    var pathSegments = map.GetAsPathSegments();
    
    return pathSegments.GetPathLengths().Max() - 1;
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

Map CreateMap(string[] input)
{
    var chars = input.Select(l => l.ToArray()).ToArray();
    return new Map(directions, chars, validDirectionsFrom, validDirectionsTo);
}

record Direction(char Label, int DeltaX, int DeltaY, char OppositeDirectionLabel);

class Map
{
    private readonly char[][] _locations;
    private readonly Dictionary<char, Direction> _directions;
    private Dictionary<char, Direction[]> _validDirectionsFrom;
    private Dictionary<char, Direction[]> _validDirectionsTo;
    private Dictionary<Point, PathSegment> _segmentsByStart = new Dictionary<Point, PathSegment>();
    
    public int Width => _locations[0].Length;
    public int Height => _locations.Length;
    
    public Map(Direction[] directions, char[][] locations, Dictionary<char, Direction[]> validDirectionsFrom, Dictionary<char, Direction[]> validDirectionsTo)
    {
        _directions = directions.ToDictionary(d => d.Label, d => d);
        _locations = locations;
        var width = locations[0].Length;
        if (locations.Any(x => x.Length != width))
        {
            throw new Exception("All lines do not have same width");
        }

        _validDirectionsFrom = validDirectionsFrom;
        _validDirectionsTo = validDirectionsTo;
    }

    public PathSegment GetAsPathSegments()
    {
        return BuildPathFrom(new Point(1, 0));
    }

    private PathSegment BuildPathFrom(Point start)
    {
        if (_segmentsByStart.TryGetValue(start, out var segment))
        {
            return segment;
        }

        List<Point> path = new List<Point>();

        var validMoves = new Queue<Point>();
        validMoves.Enqueue(start);
        while (validMoves.Count == 1)
        {
            var currentPosition = validMoves.Dequeue();
            path.Add(currentPosition);

            foreach (var validMove in GetAdjacentPoints(currentPosition)
                         .Where(x => GetChar(x.B) == '.' && IsValidMove(x) && !path.Contains(x.B)))
            {
                validMoves.Enqueue(validMove.B);
            }

            if (path.Count > Width * Height)
            {
                throw new Exception($"Found a loop within the segment starting @ {path[0]}");
            }
        }

        var newSegment = new PathSegment(path, new List<PathSegment>());
        _segmentsByStart.Add(path[0], newSegment);

        var nextLocations = GetAdjacentPoints(path[^1]).Where(x => GetChar(x.B) != '.' && IsValidMove(x)).ToList();
        if (nextLocations.Any())
        {
            var passableSlope = nextLocations.Single();
            var joinPoint = GetAdjacentPoints(passableSlope.B).Single(IsValidMove).B;

            foreach (var startOfNextSegment in GetAdjacentPoints(joinPoint).Where(IsValidMove))
            {
                newSegment.NextSegments.Add(BuildPathFrom(startOfNextSegment.B));
            }
        }

        return newSegment;
    }

    public bool IsValidMove(PointPair pair)
    {
        var charA = GetChar(pair.A);
        var charB = GetChar(pair.B);
        return charA != '#' && charB != '#' &&
            _validDirectionsFrom[charA].Contains(pair.DirectionFromAToB) &&
            _validDirectionsTo[charB].Contains(pair.DirectionFromAToB);
    }

    public char GetChar(Point p)
    {
        return _locations[p.Y][p.X];
    }

    public IEnumerable<Point> FindLabel(char label)
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (_locations[y][x] == label)
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
    
    private bool PointIsInBounds(Point p)
    {
        return p.X >= 0 && p.X < Width &&
               p.Y >= 0 && p.Y < Height;
    }
    
    public IEnumerable<PointPair> GetAdjacentPoints(Point p)
    {
        var points = _directions.Select(d => GetNextPointInDirection(d.Value, p));
        return points.Where(x => x is not null);
    }
}

record PathSegment(List<Point> Path, List<PathSegment> NextSegments)
{
    public IEnumerable<int> GetPathLengths()
    {
        if (NextSegments.Count == 0)
        {
            yield return Path.Count;
        }
        else
        {
            foreach (var pathLengthFromNextSegment in NextSegments.SelectMany(s => s.GetPathLengths()))
            {
                yield return pathLengthFromNextSegment + Path.Count + 2;
            }
        }
    }
}

record PointPair(Point A, Point B, Direction DirectionFromAToB);

record Point(int X, int Y);
