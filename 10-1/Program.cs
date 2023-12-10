var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\10-1\input.txt");
var width = lines[0].Length;

Direction[] Directions =
{
    new ('N', 0, -1, 'S'),
    new ('S', 0, 1, 'N'),
    new ('E', 1, 0, 'W'),
    new ('W', -1, 0, 'E'),
};

Dictionary<Direction, HashSet<char>> LabelsConnectingByDirection =
    new Dictionary<Direction, HashSet<char>>()
    {
        { Directions.Single(d => d.Label == 'N'), new HashSet<char> { '|', 'L', 'J' } },
        { Directions.Single(d => d.Label == 'S'), new HashSet<char> { '|', '7', 'F' } },
        { Directions.Single(d => d.Label == 'E'), new HashSet<char> { '-', 'L', 'F' } },
        { Directions.Single(d => d.Label == 'W'), new HashSet<char> { '-', 'J', '7' } },
    };

Map map = new Map(Directions, LabelsConnectingByDirection, lines.Select(s => s.ToCharArray()).ToArray());

var start = map.FindLabel('S').Single();
var locations = map.GetAdjacentConnections(start).ToArray();

bool StartConnectedEast = locations.Any(l => l.X > start.X);
bool StartConnectedWest = locations.Any(l => l.X < start.X);
bool StartConnectedSouth = locations.Any(l => l.Y > start.Y);
bool StartConnectedNorth = locations.Any(l => l.Y > start.Y);

if (locations.Length > 2)
{
    throw new Exception("Too many candidates from start");
}

var locationA = locations[0];
var locationB = locations[1];
HashSet<Point> history = new HashSet<Point> { start };

while (locationA != locationB && 
       !history.Contains(locationA) && 
       !history.Contains(locationB))
{
    var options = map.GetAdjacentConnections(locationA);
    var next = options.Single(x => !history.Contains(x));
    history.Add(locationA);
    locationA = next;
    
    options = map.GetAdjacentConnections(locationB);
    next = options.Single(x => !history.Contains(x));
    history.Add(locationB);
    locationB = next;
}
history.Add(locationA);


bool DoCrossingsVis = false;

if (DoCrossingsVis)
{
    Console.BufferHeight = 180;
    Console.SetCursorPosition(0, 0);
    for (int y = 0; y < lines.Length; y++)
    {
        Console.WriteLine(lines[y]);
    }
}

int enclosedCount = 0;
for (int y = 0; y < lines.Length; y++)
{
    for (int x = 0; x < width; x++)
    {
        var p = new Point(x, y);
        var enclosed = PointIsEnclosed(p);
        if (enclosed && !DoCrossingsVis)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write('I');
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        else if (history.Contains(p) && !DoCrossingsVis)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(lines[y][x]);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        else if(!DoCrossingsVis)
        {
            Console.Write(lines[y][x]);
        }
        if (enclosed)
        {
            enclosedCount++;
        }
    }
    if(!DoCrossingsVis) Console.WriteLine();
}

if(!DoCrossingsVis) Console.WriteLine(enclosedCount);


bool PointIsEnclosed(Point p)
{
    bool vis = false;
    if (p.X == 31 && p.Y == 54)
    {
        vis = false;
    }
    
    if (history.Contains(p)) return false;

    if (vis)
    {
        Console.SetCursorPosition(p.X, p.Y);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(lines[p.Y][p.X]);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
    var crossingsWest = GetCrossings(p, 1, 0, vis);
    var crossingsEast = GetCrossings(p, -1, 0, vis);
    var crossingsNorth = GetCrossings(p, 0, -1, vis);
    var crossingsSouth = GetCrossings(p, 0, 1, vis);

    if ((crossingsEast + crossingsWest) % 2 != 0)
    {
        Console.WriteLine($"Anomaly EW @ {p.X}, {p.Y}. {crossingsEast} + {crossingsWest}");
    }
    if ((crossingsNorth + crossingsSouth) % 2 != 0)
    {
        Console.WriteLine($"Anomaly NS @ {p.X}, {p.Y}. {crossingsNorth} + {crossingsSouth}");
    }
    
    var enclosed = crossingsEast > 0 && crossingsNorth > 0 && crossingsSouth > 0 && crossingsWest > 0 &&
                   (crossingsEast % 2 == 1 || crossingsSouth % 2 == 1 || crossingsWest % 2 == 1 || crossingsNorth % 2 == 1);

    return enclosed;
}

int GetCrossings(Point start, int deltaX, int deltaY, bool vis)
{
    var crossings = 0;
    var current = new Point(start.X + deltaX, start.Y + deltaY);

    bool connectedPositive = false;
    bool connectedNegative = false;
    bool onPartial = false;
    
    while (current.X <= width &&
           current.X >= 0 &&
           current.Y <= lines.Length &&
           current.Y >= 0)
    {
        if (history.Contains(current))
        {
            if (deltaX == 0)
            {
                var thisConnectedNegative = lines[current.Y][current.X] == 'S' ? StartConnectedWest : LabelsConnectingByDirection[Directions.Single(x => x.Label == 'W')].Contains(lines[current.Y][current.X]);
                var thisConnectedPositive = lines[current.Y][current.X] == 'S' ? StartConnectedEast : LabelsConnectingByDirection[Directions.Single(x => x.Label == 'E')].Contains(lines[current.Y][current.X]);
                
                if (thisConnectedNegative && thisConnectedPositive ||
                    (onPartial && (connectedPositive && thisConnectedNegative) || (connectedNegative && thisConnectedPositive)))
                {
                    crossings++;
                    connectedNegative = false;
                    connectedPositive = false;
                    onPartial = false;
                    if (vis)
                    {
                        Console.SetCursorPosition(current.X, current.Y);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(lines[current.Y][current.X]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
                else if (thisConnectedNegative || thisConnectedPositive)
                {
                    if (onPartial && 
                        thisConnectedNegative && connectedNegative ||
                        thisConnectedPositive && connectedPositive)
                    {
                        connectedNegative = false;
                        connectedPositive = false;
                        onPartial = false;
                        if (vis)
                        {
                            Console.SetCursorPosition(current.X, current.Y);
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.Write(lines[current.Y][current.X]);
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                    else
                    {
                        connectedNegative = connectedNegative || thisConnectedNegative;
                        connectedPositive = connectedPositive || thisConnectedPositive;
                        onPartial = true;
                        if (vis)
                        {
                            Console.SetCursorPosition(current.X, current.Y);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(lines[current.Y][current.X]);
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                }
            }
            
            if (deltaY == 0)
            {
                var thisConnectedNegative = lines[current.Y][current.X] == 'S' ? StartConnectedNorth : LabelsConnectingByDirection[Directions.Single(x => x.Label == 'N')].Contains(lines[current.Y][current.X]);
                var thisConnectedPositive = lines[current.Y][current.X] == 'S' ? StartConnectedSouth : LabelsConnectingByDirection[Directions.Single(x => x.Label == 'S')].Contains(lines[current.Y][current.X]);

                if (thisConnectedNegative && thisConnectedPositive ||
                    (onPartial && (connectedPositive && thisConnectedNegative) || (connectedNegative && thisConnectedPositive)))
                {
                    crossings++;
                    connectedNegative = false;
                    connectedPositive = false;
                    onPartial = false;
                    if (vis)
                    {
                        Console.SetCursorPosition(current.X, current.Y);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(lines[current.Y][current.X]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
                else if (thisConnectedNegative || thisConnectedPositive)
                {
                    if (onPartial && 
                        thisConnectedNegative && connectedNegative ||
                        thisConnectedPositive && connectedPositive)
                    {
                        connectedNegative = false;
                        connectedPositive = false;
                        onPartial = false;
                        if (vis)
                        {
                            Console.SetCursorPosition(current.X, current.Y);
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.Write(lines[current.Y][current.X]);
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                    else
                    {
                        connectedNegative = connectedNegative || thisConnectedNegative;
                        connectedPositive = connectedPositive || thisConnectedPositive;
                        onPartial = true;
                        if (vis)
                        {
                            Console.SetCursorPosition(current.X, current.Y);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(lines[current.Y][current.X]);
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                }
            }
        }
        current = new Point(current.X + deltaX, current.Y + deltaY);
    }
    return crossings;
}


record Direction(char Label, int DeltaX, int DeltaY, char OppositeDirectionLabel);


class Map
{
    private readonly char[][] _locations;
    private readonly Dictionary<char, Direction> _directions;
    private readonly Dictionary<Direction, HashSet<char>> _connectingLabelsByDirection;
    
    public Map(Direction[] directions, Dictionary<Direction, HashSet<char>> connectingLabelsByDirection, char[][] locations)
    {
        _directions = directions.ToDictionary(d => d.Label, d => d);
        _connectingLabelsByDirection = connectingLabelsByDirection;
        _locations = locations;
        var width = locations[0].Length;
        if (locations.Any(x => x.Length != width))
        {
            throw new Exception("All lines do not have same width");
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
    
    public IEnumerable<Point> GetAdjacentConnections(Point p)
    {
        foreach (var point in GetAdjacentPoints(p)
                     .Where(AreConnected))
        {
            yield return point.B;
        }
    }
    
    private bool AreConnected(PointPair points)
    {
        var (p1, p2, direction) = points;
        var label1 = _locations[p1.Y][p1.X];
        var label2 = _locations[p2.Y][p2.X];

        return (_connectingLabelsByDirection[direction].Contains(label1) || label1 == 'S') &&
               (_connectingLabelsByDirection[_directions[direction.OppositeDirectionLabel]].Contains(label2) || label2 == 'S');
    }

    private PointPair? GetNextPointInDirection(Direction d, Point p)
    {
        var nextPoint = new Point(p.X + d.DeltaX, p.Y + d.DeltaY);
        return PointIsInBounds(nextPoint) ? new PointPair(p, nextPoint, d) : null;
    }
    
    private bool PointIsInBounds(Point p)
    {
        return p.X >= 0 && p.X < _locations[0].Length &&
               p.Y >= 0 && p.Y < _locations.Length;
    }
    
    private IEnumerable<PointPair> GetAdjacentPoints(Point p)
    {
        return _directions.Select(d => GetNextPointInDirection(d.Value, p)).Where(x => x is not null);
    }
}

record PointPair(Point A, Point B, Direction DirectionFromAToB);

record Point(int X, int Y);
