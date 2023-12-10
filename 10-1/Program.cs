var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\10-1\input.txt");
var width = lines[0].Length;

var start = FindStart();

HashSet<char> ConnectedNorth = new HashSet<char> { 'S', '|', 'L', 'J' };
HashSet<char> ConnectedSouth = new HashSet<char> { 'S', '|', '7', 'F' };
HashSet<char> ConnectedEast = new HashSet<char> { 'S', '-', 'L', 'F' };
HashSet<char> ConnectedWest = new HashSet<char> { 'S', '-', 'J', '7' };

var locations = GetAdjacentConnections(start).ToArray();
if (locations.Length > 2)
{
    throw new Exception("Too many candidates from start");
}

var locationA = locations[0];
var locationB = locations[1];
HashSet<Point> history = new HashSet<Point> { start };
var count = 1;

while (locationA != locationB && 
       !history.Contains(locationA) && 
       !history.Contains(locationB))
{
    Console.WriteLine($"Step {count} - {locationA.X}, {locationA.Y} and {locationB.X}, {locationB.Y}");
    
    var options = GetAdjacentConnections(locationA);
    var next = options.Single(x => !history.Contains(x));
    history.Add(locationA);
    locationA = next;
    
    options = GetAdjacentConnections(locationB);
    next = options.Single(x => !history.Contains(x));
    history.Add(locationB);
    locationB = next;
    
    count++;
}


Console.WriteLine(count);


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
