using System.Collections.Immutable;

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

AssertFor(@"R 2 (#0dc571)
D 1 (#0dc571)
R 1 (#5713f0)
D 1 (#d2c081)
R 3 (#59c680)
D 7 (#411b91)
L 2 (#8ceee2)
U 4 (#caa173)
L 2 (#1b58a2)
D 4 (#caa171)
L 2 (#1b58a2)
U 9 (#caa171)", 59, false);

AssertFor(@"R 6 (#70c710)
D 4 (#0dc571)
L 2 (#5713f0)
U 2 (#d2c081)
L 2 (#59c680)
D 4 (#411b91)
R 4 (#8ceee2)
D 3 (#caa173)
L 6 (#1b58a2)
U 9 (#caa171)", 64, false);

AssertFor(@"R 5 (#70c710)
D 1 (#0dc571)
R 3 (#5713f0)
D 8 (#d2c081)
R 3 (#59c680)
D 3 (#411b91)
L 5 (#8ceee2)
U 9 (#caa173)
L 2 (#1b58a2)
D 9 (#caa171)
L 3 (#caa171)
U 7 (#caa171)
L 1 (#caa171)
U 5 (#caa171)", 110, false);

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
U 2 (#7a21e3)", 62, false);

AssertFor(File.ReadAllText(@"C:\Repos\advent-of-code-2023\18-1\input.txt"), 33491, false);

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
U 2 (#7a21e3)", 952408144115, true);

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\18-1\input.txt");
Console.WriteLine(RunFor(lines, true, true));

long RunFor(string[] input, bool logging, bool doPart2)
{
    var trench = DigTrench(input, doPart2);
    var size = DigInterior(trench);
    return size;
}

long DigInterior((Line[] Lines, ImmutableSortedSet<long> VerticalLineXValues, ImmutableSortedSet<long> HorizontalLineYValues) trench)
{
    Point topLeft = new Point(trench.Lines.Min(t => Math.Min(t.P1.X, t.P2.X)), trench.Lines.Min(t => Math.Min(t.P1.Y, t.P2.Y)));
    Point bottomRight = new Point(trench.Lines.Max(t => Math.Max(t.P1.X, t.P2.X)), trench.Lines.Max(t => Math.Max(t.P1.Y, t.P2.Y)));
    
    var result = 0l;
    long lastLineFromTrenchValues = long.MinValue;
    
    long currentY = topLeft.Y;
    
    while (currentY <= bottomRight.Y)
    {
        var nextY = currentY + 1;
        
        if (trench.HorizontalLineYValues.Contains(currentY))
        {
            lastLineFromTrenchValues = currentY;
        }
        if (currentY > lastLineFromTrenchValues + 1)
        {
            var potentialValue = GetNextValue(currentY, trench.HorizontalLineYValues, true);
            if (potentialValue.HasValue)
            {
                nextY = potentialValue.Value - 1 >= currentY + 1 ? potentialValue.Value - 1 : currentY + 1;
            }
        }

        var lineArea = GetTrenchAreaForRow(currentY, trench.Lines, topLeft.X, bottomRight.X);
        var repeats = nextY - currentY;
        //Console.WriteLine($"{lineArea.Item1} points in trench on line {currentY} x {repeats} = {lineArea.Item1 * repeats}");
        
        result += lineArea * repeats;
        currentY = nextY;

        if (currentY == bottomRight.Y)
        {
            int j = 0;
        }
    }
    Console.WriteLine($"{result}");
    return result;
}

long GetTrenchAreaForRow(long currentY, Line[] lines, long minX, long maxX)
{
    var linesOnRow = GetLinesOnRow(lines, currentY);
    var position = minX;
    var lastPosition = minX;
    long result = 0l;
    bool inTrench = false;
    
    while (position <= maxX)
    {
        var currentPoint = new Point(position, currentY);

        // Fill in the previous segment
        var fillLength = position - lastPosition;
        if (inTrench)
        {
            result += fillLength;
        }

        var horizontalLine = GetHorizontalLine(linesOnRow, currentPoint);
        if (horizontalLine != null)
        {
            var length = horizontalLine.MaxX - horizontalLine.MinX + 1;
            result += length;
            position += length;
            
            inTrench = OnAnEndLine(linesOnRow, currentPoint) ? inTrench : !inTrench;
        }
        else if (HitTrench(linesOnRow, currentPoint))
        {
            result++;
            position++;
            
            inTrench = !inTrench;
        }
        
        if (position >= maxX) break;
        lastPosition = position;
        var nextLine = linesOnRow.Where(l => l.MinX > position).ToArray();
        position = nextLine.Length > 0 ? nextLine.Min(l => l.MinX) : maxX;
    }

    return result;
}

bool OnAnEndLine(Line[] lines, Point p)
{
    var match = lines.Single(l => l.P1.Y == l.P2.Y && l.P1.Y == p.Y &&
                          l.MinX <= p.X && l.MaxX >= p.X);
    var connections = lines.Where(l =>
        l.P1.X == l.P2.X &&
        (l.P1 == match.P1 || l.P1 == match.P2 || l.P2 == match.P1 || l.P2 == match.P2));

    return connections.All(l => l.MinY >= match.P1.Y && l.MaxY >= match.P1.Y) ||
        connections.All(l => l.MinY <= match.P1.Y && l.MaxY <= match.P1.Y);
}

Line? GetHorizontalLine(Line[] lines, Point p)
{
    var query = lines.Where(l => l.P1.Y == l.P2.Y && l.P1.Y == p.Y &&
                   l.MinX <= p.X && l.MaxX >= p.X).ToArray();
    return query.Any() ? query.Single() : null;
}

Line[] GetLinesOnRow(Line[] lines, long y)
{
    return lines.Where(l => l.MinY <= y && l.MaxY >= y).ToArray();
}


long? GetNextValue(long currentValue, ImmutableSortedSet<long> lineValues, bool larger)
{
    try
    {
        if (larger)
        {
            return lineValues.Where(x => x > currentValue).Min();
        }

        return lineValues.Where(x => x < currentValue).Max();
    }
    catch (InvalidOperationException)
    {
        return null;
    }
}

bool HitTrench(Line[] trench, Point p)
{
    var matchingHorizontalLines = trench.Where(t => t.P1.Y == p.Y && t.P2.Y == p.Y).ToArray();
    var matchingVerticalLines = trench.Where(t => t.P1.X == p.X && t.P2.X == p.X).ToArray();
    if (matchingHorizontalLines.Any(l => l.MinX <= p.X && l.MaxX >= p.X))
    {
        return true;
    }
    return matchingVerticalLines.Any(l => l.MinY <= p.Y && l.MaxY >= p.Y);
} 

(Line[] Trench, ImmutableSortedSet<long> VerticalLineXValues, ImmutableSortedSet<long> HorizontalLineYValues) DigTrench(string[] strings, bool doPart2)
{
    long x = 0;
    long y = 0;
    var trench = new List<Line>();
    var verticalLineXValues = new List<long>();
    var horizontalLineYValues = new List<long>();

    foreach (var line in strings)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Direction direction;
        switch (parts[0])
        {
            case "U":
                direction = n;
                verticalLineXValues.Add(x);
                break;
            case "D":
                direction = s;
                verticalLineXValues.Add(x);
                break;
            case "L":
                direction = w;
                horizontalLineYValues.Add(y);
                break;
            case "R":
                direction = e;
                horizontalLineYValues.Add(y);
                break;
            default:
                throw new Exception("Bad label");
        }

        var length = long.Parse(parts[1]);

        if (doPart2)
        {
            var distancePart = parts[2].Remove(parts[2].Length - 2).Substring(2);
            var directionPart = parts[2].Substring(parts[2].Length - 2, 1);
            
            switch (directionPart)
            {
                case "3":
                    direction = n;
                    verticalLineXValues.Add(x);
                    break;
                case "1":
                    direction = s;
                    verticalLineXValues.Add(x);
                    break;
                case "2":
                    direction = w;
                    horizontalLineYValues.Add(y);
                    break;
                case "0":
                    direction = e;
                    horizontalLineYValues.Add(y);
                    break;
                default:
                    throw new Exception("Bad label");
            }
            length = Convert.ToInt64(distancePart,16);
        }

        var nextX = x + direction.DeltaX * length;
        var nextY = y + direction.DeltaY * length;
        trench.Add(new Line(new Point(x, y), new Point(nextX, nextY)));
        x = nextX;
        y = nextY;
    }

    return (trench.ToArray(), verticalLineXValues.Order().ToImmutableSortedSet(), horizontalLineYValues.Order().ToImmutableSortedSet());
}

void AssertFor(string input, long expectedResult, bool doPart2)
{
    var lines = input.Split(System.Environment.NewLine);
    var result = RunFor(lines, false, doPart2);
    if (result != expectedResult)
    {
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
        throw new Exception($"Result was {result} but expected {expectedResult}");
    }
}

record Line(Point P1, Point P2)
{
    public long MinX => Math.Min(P1.X, P2.X);
    public long MaxX => Math.Max(P1.X, P2.X);
    public long MinY => Math.Min(P1.Y, P2.Y);
    public long MaxY => Math.Max(P1.Y, P2.Y);
}

record Direction(char Label, int DeltaX, int DeltaY, char OppositeDirectionLabel);


record Point(long X, long Y);
