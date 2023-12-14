using System.Diagnostics;

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\14-1\input.txt");

AssertFor(@"O....#....
O.OO#....#
.....##...
OO.#O....O
.O.....O#.
O.#..O.#.#
..O..#O..O
.......O..
#....###..
#OO..#....", 136);

Console.Write(RunFor(lines));


long RunFor(string[] input)
{
    var rows = input.Select(s => s.ToCharArray()).ToArray();

    Map map = new Map(rows);
    var total = 0l;
    foreach (var col in map.GetColumns())
    {
        var previousSpaces = 0;
        var colScore = 0l;
        for(int i = 0; i < col.Length; i++)
        {
            switch (col[i])
            {
                case '.':
                    previousSpaces++;
                    break;
                case '#':
                    previousSpaces = 0;
                    break;
                case 'O':
                    colScore += col.Length - (i - previousSpaces);
                    break;
                default:
                    throw new Exception("ARG!");
            }
        }
        total += colScore;
        Console.WriteLine(colScore);
    }

    Console.WriteLine(total);
    return total;
}

void AssertFor(string input, long expected)
{
    Debug.Assert(RunFor(input.Split(Environment.NewLine)) == expected);
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
    
    public IEnumerable<char[]> GetColumns()
    {
        for (int c = 0; c < _locations[0].Length; c++)
        {
            char[] result = new char[_locations.Length];
            for (int r = 0; r < _locations.Length; r++)
            {
                result[r] = _locations[r][c];
            }
            yield return result;
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