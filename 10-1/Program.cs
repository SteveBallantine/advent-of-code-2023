using System.Diagnostics;
using System.Security.Cryptography;

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\10-1\input.txt");
var width = lines[0].Length;

var start = FindStart();

HashSet<char> ConnectedNorth = new HashSet<char> { 'S', '|', 'L', 'J' };
HashSet<char> ConnectedSouth = new HashSet<char> { 'S', '|', '7', 'F' };
HashSet<char> ConnectedEast = new HashSet<char> { 'S', '-', 'L', 'F' };
HashSet<char> ConnectedWest = new HashSet<char> { 'S', '-', 'J', '7' };

var locations = GetAdjacentConnections(start).ToArray();
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
    var options = GetAdjacentConnections(locationA);
    var next = options.Single(x => !history.Contains(x));
    history.Add(locationA);
    locationA = next;
    
    options = GetAdjacentConnections(locationB);
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
    if (p.X == 69 && p.Y == 58)
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
                
                var thisConnectedNegative = (lines[current.Y][current.X] == 'S' && StartConnectedWest) || ConnectedWest.Contains(lines[current.Y][current.X]);
                var thisConnectedPositive = (lines[current.Y][current.X] == 'S' && StartConnectedEast) || ConnectedEast.Contains(lines[current.Y][current.X]);
                
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
                var thisConnectedNegative = (lines[current.Y][current.X] == 'S' && StartConnectedNorth) || ConnectedNorth.Contains(lines[current.Y][current.X]);
                var thisConnectedPositive = (lines[current.Y][current.X] == 'S' && StartConnectedSouth) || ConnectedSouth.Contains(lines[current.Y][current.X]);

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


Point FindStart()
{
    for (var y = 0; y < lines.Length; y++)
    {
        if (lines[y].Length != width)
        {
            throw new Exception($"unexpected line width {lines[y].Length} chars in line {y} vs expected {width} chars");
        }

        for (var x = 0; x < width; x++)
        {
            if (lines[y][x] == 'S')
            {
                return new Point(x, y);
            }
        }
    }

    throw new Exception("No Start!");
}


IEnumerable<Point> GetAdjacentConnections(Point p)
{
    if (p.X < width - 1)
    {
        var east = new Point(p.X + 1, p.Y);
        if (AreConnected(p, east))
        {
            yield return east;
        }
    }

    if (p.X > 0)
    {
        var west = new Point(p.X - 1, p.Y);
        if (AreConnected(p, west))
        {
            yield return west;
        }
    }

    if (p.Y < lines.Length - 1)
    {
        var south = new Point(p.X, p.Y + 1);
        if (AreConnected(p, south))
        {
            yield return south;
        }
    }

    if (p.Y > 0)
    {
        var north = new Point(p.X, p.Y - 1);
        if (AreConnected(p, north))
        {
            yield return north;
        }
    }
}

bool AreConnected(Point p1, Point p2)
{
    // Should really check for adjacency...
    var char1 = lines[p1.Y][p1.X];
    var char2 = lines[p2.Y][p2.X];

    if (p1.X == p2.X)
    {
        if (p1.Y > p2.Y)
        {
            // P2 is North of P1
            return ConnectedNorth.Contains(char1) && ConnectedSouth.Contains(char2);
        }
        // P2 is South of P1
        return ConnectedSouth.Contains(char1) && ConnectedNorth.Contains(char2);
    }
    
    if (p1.X > p2.X)
    {
        // P2 is West of P1
        return ConnectedWest.Contains(char1) && ConnectedEast.Contains(char2);
    }
    // P1 is East of P2
    return ConnectedEast.Contains(char1) && ConnectedWest.Contains(char2);
}

record Point(int X, int Y);
