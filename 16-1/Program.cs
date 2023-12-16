using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;

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

AssertFor(@".|...\....
|.-.\.....
.....|-...
........|.
..........
.........\
..../.\\..
.-.-/..|..
.|....-|.\
..//.|....", 51);

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\16-1\input.txt");
Console.WriteLine(RunFor(lines, true));

long RunFor(string[] input, bool logging)
{
    int max = 0;
    
    for (int x = 0; x < input[0].Length; x++)
    {
        var map = CreateMap(input);
        map.StartBeam(new Beam(new Point(x, 0), s));
        if (map.GetEnergizedCount > max) max = map.GetEnergizedCount;
        
        var map2 = CreateMap(input);
        map2.StartBeam(new Beam(new Point(x, map2.Height - 1), n));
        if (map2.GetEnergizedCount > max) max = map2.GetEnergizedCount;
    }
    for (int y = 0; y < input.Length; y++)
    {
        var map = CreateMap(input);
        map.StartBeam(new Beam(new Point(0, y), e));
        if (map.GetEnergizedCount > max) max = map.GetEnergizedCount;
        
        var map2 = CreateMap(input);
        map2.StartBeam(new Beam(new Point(map.Width - 1, y), w));
        if (map2.GetEnergizedCount > max) max = map2.GetEnergizedCount;
    }

    /*var map = CreateMap(input);
    map.StartBeam(new Beam(new Point(0, 0), e));
    max = map.GetEnergizedCount;*/
    
    /*foreach (var line in map.GetEnergized)
    {
        foreach (var point in line)
        {
            Console.Write(point ? '#' : '.');
        }
        Console.WriteLine();
    }*/

    return max;
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
    Dictionary<(char, Direction), Direction[]> labelBeamDirectionChanges =
        new Dictionary<(char, Direction), Direction[]>()
        {
            { ('.', n), new[] { n } },
            { ('.', s), new[] { s } },
            { ('.', e), new[] { e } },
            { ('.', w), new[] { w } },
            { ('|', n), new[] { n } },
            { ('|', s), new[] { s } },
            { ('|', e), new[] { n, s } },
            { ('|', w), new[] { n, s } },
            { ('-', n), new[] { e, w } },
            { ('-', s), new[] { e, w } },
            { ('-', e), new[] { e } },
            { ('-', w), new[] { w } },
            { ('/', n), new[] { e } },
            { ('/', s), new[] { w } },
            { ('/', e), new[] { n } },
            { ('/', w), new[] { s } },
            { ('\\', n), new[] { w } },
            { ('\\', s), new[] { e } },
            { ('\\', e), new[] { s } },
            { ('\\', w), new[] { n } },
        };

    return new Map(directions, labelBeamDirectionChanges, input.Select(s => s.ToCharArray()).ToArray());
}

record Direction(char Label, int DeltaX, int DeltaY, char OppositeDirectionLabel);


class Map
{
    private readonly char[][] _locations;
    private readonly Dictionary<char, Direction> _directions;
    private readonly Dictionary<(char, Direction), Direction[]> _labelBeamDirectionChanges;

    private readonly bool[][] _energized;
    private readonly HashSet<Direction>[][] _entryDirections;
    
    public int Width => _locations[0].Length;
    public int Height => _locations.Length;
    
    public Map(Direction[] directions, Dictionary<(char, Direction), Direction[]> labelBeamDirectionChanges, char[][] locations)
    {
        _directions = directions.ToDictionary(d => d.Label, d => d);
        _labelBeamDirectionChanges = labelBeamDirectionChanges;
        _locations = locations;
        var width = locations[0].Length;
        if (locations.Any(x => x.Length != width))
        {
            throw new Exception("All lines do not have same width");
        }

        _energized = new bool[Height][];
        _entryDirections = new HashSet<Direction>[Height][];
        for(int i = 0; i < Height; i++)
        {
            _energized[i] = new bool[Width];
            _entryDirections[i] = new HashSet<Direction>[Width];
            for(int j = 0; j < Width; j++)
            {
                _entryDirections[i][j] = new HashSet<Direction>();
            }
        }
    }

    public int GetEnergizedCount => _energized.Sum(r => r.Count(e => e));

    public bool[][] GetEnergized => _energized;

    public void StartBeam(Beam beam)
    {
        Channel<Beam> beams = Channel.CreateUnbounded<Beam>(new UnboundedChannelOptions());
        beams.Writer.TryWrite(beam);

        while (beams.Reader.Count > 0)
        {
            if(beams.Reader.TryRead(out var thisBeam))
            {
                foreach (var nextBeam in RunBeam(thisBeam))
                {
                    beams.Writer.TryWrite(nextBeam);
                }
            }
        }
    }
    
    private IEnumerable<Beam> RunBeam(Beam beam)
    {
        _energized[beam.Location.Y][beam.Location.X] = true;
        _entryDirections[beam.Location.Y][beam.Location.X].Add(beam.Direction);
        
        var newDirections = _labelBeamDirectionChanges[(_locations[beam.Location.Y][beam.Location.X], beam.Direction)];
        foreach (var newDirection in newDirections)
        {
            var nextPosition = new Point(beam.Location.X + newDirection.DeltaX, beam.Location.Y + newDirection.DeltaY);
            if (PointIsInBounds(nextPosition) && 
                (!_energized[nextPosition.Y][nextPosition.X] || !_entryDirections[nextPosition.Y][nextPosition.X].Contains(newDirection)))
            {
                yield return new Beam(nextPosition, newDirection);
            }
        }
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
    
    private IEnumerable<PointPair> GetAdjacentPoints(Point p)
    {
        return _directions.Select(d => GetNextPointInDirection(d.Value, p)).Where(x => x is not null);
    }
}

record Beam(Point Location, Direction Direction);

record PointPair(Point A, Point B, Direction DirectionFromAToB);

record Point(int X, int Y);
