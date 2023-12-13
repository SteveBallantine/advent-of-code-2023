using System.Collections;
using System.Text.RegularExpressions;

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\13-1\input.txt");
var blocks = BreakIntoMaps(lines);

var result = 0l;

foreach (var block in blocks)
{
    foreach (var b in block)
    {
        Console.WriteLine(b);
    }
    var rows = block.Select(s => s.ToCharArray()).ToArray();
    
    Map map = new Map(rows);

    var columns = map.GetColumns().ToArray();

    var colMap = ConvertToIntArray(columns);
    var rowMap = ConvertToIntArray(rows);
    Console.WriteLine("row map");

    var rPoint = FindReflectionPoint(colMap);
    if (rPoint != null)
    {
        Console.WriteLine($"vertical reflection @ {rPoint}");
        result += rPoint.Value;
    }

    rPoint = FindReflectionPoint(rowMap);
    if (rPoint != null)
    {
        Console.WriteLine($"horizontal reflection @ {rPoint}");
        result += rPoint.Value * 100;
    }
}

Console.WriteLine(result);


IEnumerable<string[]> BreakIntoMaps(string[] data)
{
    List<string> next = new List<string>();
    foreach (var row in data)
    {
        if (string.IsNullOrWhiteSpace(row))
        {
            yield return next.ToArray();
            next.Clear();
        }
        else
        {
            next.Add(row);
        }
    }

    yield return next.ToArray();
}

int? FindReflectionPoint(int[] data)
{
    for (int i = 0; i < data.Length; i++)
    {
        int checkPoint1Offset = 0;
        int checkPoint2Offset = 1;
        bool hit = false;
        
        while (
            i + checkPoint1Offset >= 0 &&
            i + checkPoint2Offset < data.Length &&
            data[i + checkPoint1Offset] == data[i + checkPoint2Offset])
        {
            checkPoint1Offset--;
            checkPoint2Offset++;
            hit = true;
        }

        if (hit && (i + checkPoint1Offset < 0 || i + checkPoint2Offset >= data.Length))
        {
            return i + 1;
        }
    }

    return null;
}


int[] ConvertToIntArray(char[][] data)
{
    int[] result = new int[data.Length];
    for(int i = 0; i < data.Length; i++)
    {
        for(int j = 0 ;j < data[i].Length; j++)
        {
            if (data[i][j] == '#')
            {
                result[i] |= 1 << j;
            }
        }
    }
    return result;
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