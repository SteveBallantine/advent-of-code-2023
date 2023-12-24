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
    { '>', new Direction[] { e, w, n, s } },
    { '<', new Direction[] { w, e, n, s } },
    { '^', new Direction[] { n, s, e, w } },
    { 'v', new Direction[] { s, n, e, w } },
};
Dictionary<char, Direction[]> validDirectionsTo = new Dictionary<char, Direction[]>
{
    { '.', new Direction[] { n, s, e, w } },
    { '>', new Direction[] { n, s, e, w } },
    { '<', new Direction[] { n, s, w, e } },
    { '^', new Direction[] { e, w, n, s } },
    { 'v', new Direction[] { e, w, s, n } },
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

AssertFor(exampleInput, 154);

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\23-1\input.txt");
Console.WriteLine(RunFor(lines, true));

long RunFor(string[] input, bool logging)
{
    var map = CreateMap(input);
    var pathSegments = map.GetAsPathSegments();
    
    var results = pathSegments.GetPathLengths(new Node[0]);
    var temp = results.ToList();

    /*var path = results.OrderByDescending(x => x.Count).First();
    for (int i = 0; i < map.Height; i++)
    {
        for (int j = 0; j < map.Width; j++)
        {
            var point = new Point(j, i);
            Console.Write(path.Contains(point) ? "O" : map.GetChar(point));
        }
        Console.WriteLine();
    }*/

    return temp.Max() - 1;
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
    private Dictionary<Point, PathSegment> _segmentsByEnd = new Dictionary<Point, PathSegment>();
    private Dictionary<Point, Node> _nodes;
    
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
        return CreateGraph(new Point(1, 0));
    }
    
    private PathSegment CreateGraph(Point start)
    {
        _nodes = new Dictionary<Point, Node>();
        for (int y = 0; y < Height; y++)
        {
            for(int x = 0; x < Width; x++)
            {
                var point = new Point(x, y);
                if (GetChar(point) == '.' &&
                   _directions.All(d => GetNextPointInDirection(d.Value, point) != null && GetChar(GetNextPointInDirection(d.Value, point).B) != '.'))
                {
                    _nodes.Add(point, new Node(point, new List<PathSegment>()));
                }
            }
        }

        foreach (var node in _nodes)
        {
            foreach (var startPoint in _directions.Select(d => GetNextPointInDirection(d.Value, node.Key).B).Where(b => GetChar(b) != '#'))
            {
                node.Value.ConnectedPaths.Add(BuildPathFrom(startPoint, node.Key));
            }
        }

        return _segmentsByEnd[new Point(1, 0)];
    }

    private PathSegment BuildPathFrom(Point start, Point? lastJoinPoint)
    {
        if (_segmentsByStart.TryGetValue(start, out var segment))
        {
            return segment;
        }
        if (_segmentsByEnd.TryGetValue(start, out var segment2))
        {
            return segment2;
        }

        List<Point> path = new List<Point>();

        var validMoves = new Queue<Point>();
        validMoves.Enqueue(start);
        while (validMoves.Count == 1)
        {
            var currentPosition = validMoves.Dequeue();
            path.Add(currentPosition);

            foreach (var validMove in GetAdjacentPoints(currentPosition)
                         .Where(x => x.B != lastJoinPoint && GetChar(x.B) == '.' && IsValidMove(x) && !path.Contains(x.B)))
            {
                validMoves.Enqueue(validMove.B);
            }

            if (path.Count > Width * Height)
            {
                throw new Exception($"Found a loop within the segment starting @ {path[0]}");
            }
        }

        Point? joinPoint = null;
        var nextLocations = GetAdjacentPoints(path[^1]).Where(x => GetChar(x.B) != '.' && IsValidMove(x)).ToList();
        if (nextLocations.Any())
        {
            var passableSlope = nextLocations.Single();
            path.Add(passableSlope.B);
            joinPoint = GetAdjacentPoints(path[^1]).Single(x => !path.Contains(x.B) && IsValidMove(x)).B;
        }

        var joinPointList = new List<Point>();
        if(joinPoint != null) joinPointList.Add(joinPoint);
        if(lastJoinPoint != null) joinPointList.Add(lastJoinPoint);
        
        var newSegment = new PathSegment(path, _nodes[joinPointList[0]], joinPointList.Count == 2 ? _nodes[joinPointList[1]] : null);
        _segmentsByStart.Add(path[0], newSegment);
        _segmentsByEnd.Add(path[^1], newSegment);
        
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

record PathSegment(List<Point> Path, Node NodeA, Node? NodeB)
{
    public IEnumerable<int> GetPathLengths(Node[] nodesUsed)
    {
        if (NodeB is null && nodesUsed.Length > 0)
        {
            yield return Path.Count;
        }
        else
        {
            Node? nextNode = null;
            if (!nodesUsed.Contains(NodeA)) nextNode = NodeA; 
            if (NodeB != null && !nodesUsed.Contains(NodeB)) nextNode = NodeB;

            if (nextNode != null)
            {
                var validNextSegments = nextNode.ConnectedPaths.Where(x => x != this);
                var updatedJointPointsUsed = nodesUsed.Append(nextNode).ToArray();

                foreach (var nextSegment in validNextSegments)
                {
                    foreach (var length in nextSegment.GetPathLengths(updatedJointPointsUsed))
                    {
                        yield return length + Path.Count + 1;
                    }
                }
            }
        }
    }
}

record Node(Point Location, List<PathSegment> ConnectedPaths);

record PointPair(Point A, Point B, Direction DirectionFromAToB);

record Point(int X, int Y);
