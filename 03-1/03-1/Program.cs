var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\03-1\03-1\input.txt");
var width = lines[0].Length;

var partNumberCandidates = GetPartNumberCandidates();

DoPart1(lines, partNumberCandidates);
DoPart2(lines, partNumberCandidates);


void DoPart2(string[] lines, PartNumberCandidate?[,] candidates)
{
    var ratios = new List<int>();
    for (var y = 0; y < lines.Length; y++)
    {
        for (var x = 0; x < lines[y].Length; x++)
        {
            if (lines[y][x]=='*')
            {
                Console.WriteLine($"Symbol '{lines[y][x]}' found at line {y}, char {x}");
                var parts = GetAdjacentNumbers(lines, x, y, candidates).ToArray();
                if (parts.Length == 2)
                {
                    ratios.Add(parts[0] * parts[1]);
                }
            }
        }
    }

    Console.WriteLine(ratios.Sum());
}

void DoPart1(string[] lines, PartNumberCandidate?[,] candidates)
{
    var partNumbers = new List<int>();
    for (var y = 0; y < lines.Length; y++)
    {
        for (var x = 0; x < lines[y].Length; x++)
        {
            if (IsSymbol(lines[y][x]))
            {
                Console.WriteLine($"Symbol '{lines[y][x]}' found at line {y}, char {x}");
                partNumbers.AddRange(GetAdjacentNumbers(lines, x, y, candidates));
            }
        }
    }

    Console.WriteLine(partNumbers.Sum());
}

bool IsSymbol(char x) => !char.IsDigit(x) && x != '.';

IEnumerable<int> GetAdjacentNumbers(string[] lines, int x, int y, PartNumberCandidate?[,] candidates)
{
    var returned = new HashSet<PartNumberCandidate>();
    for (int i = y - 1; i <= y + 1 && i < lines.Length; i++)
    {
        for (int j = x - 1; j <= x + 1 && j < lines[0].Length; j++)
        {
            var candidate = candidates[j, i];
            if (candidate is not null &&
                !returned.Contains(candidate))
            {
                Console.WriteLine(candidate.Number);
                returned.Add(candidate);
                yield return candidate.Number;
            }
        }
    }
}


PartNumberCandidate?[,] GetPartNumberCandidates()
{
    var candidates = new PartNumberCandidate?[lines.Length, lines[0].Length];
    for (var y = 0; y < lines.Length; y++)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(lines[y]);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        if (lines[y].Length != width)
        {
            throw new Exception($"unexpected line width {lines[y].Length} chars in line {y} vs expected {width} chars");
        }

        for (var x = 0; x < width; x++)
        {
            if (char.IsDigit(lines[y][x]))
            {
                var candidate = GetPartNumberCandidate(lines, x, y);
                var candidateDigitCount = candidate.Number.ToString().Length;
                for (int n = x; n < x + candidateDigitCount; n++)
                {
                    candidates[n, y] = candidate;
                }
                x += candidateDigitCount - 1;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(candidate.Number);
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            else
            {
                Console.Write(lines[y][x]);
            }
        }
        Console.WriteLine();
    }

    return candidates;
}

PartNumberCandidate GetPartNumberCandidate(string[] lines, int x, int y)
{
    var end = x;
    while (end < lines[y].Length && char.IsDigit(lines[y][end]))
    {
        end++;
    }
    
    return new PartNumberCandidate(int.Parse(lines[y][x..end]));
}

record PartNumberCandidate(int Number);