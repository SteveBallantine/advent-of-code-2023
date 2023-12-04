using System.Linq;

var text = File.ReadAllText(@"C:\Repos\advent-of-code-2023\01-1\input.txt");

var updatedLines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
    .Select(ConvertNumericWordsToDigits);
var result = updatedLines
    .Select(x => GetNumberFromDigits(GetFirstAndLastDigitsFromLine(x)))
    .Sum();
Console.WriteLine(result);

string ConvertNumericWordsToDigits(string line) 
{
    var mapping = new System.Collections.Generic.Dictionary<string, char>() 
    {
        {"one", '1'},
        {"two", '2'},
        {"three", '3'},
        {"four", '4'}, 
        {"five", '5'}, 
        {"six", '6'}, 
        {"seven", '7'},
        {"eight", '8'},
        {"nine", '9'}
    };

    Console.WriteLine(line);
    Func<string, int, IOrderedEnumerable<(string, int)>> GetCandidates = (line, minimumPos) =>
    {
        if(minimumPos >= line.Length) { return Enumerable.Empty<(string, int)>().OrderBy(x => x); }

        return mapping.Select(mappingEntry => (mappingEntry.Value.ToString(), line.IndexOf(mappingEntry.Key, minimumPos > 0 ? minimumPos : 0)))
            .Where(x => x.Item2 > minimumPos)
            .OrderBy(x => x.Item2);
    };

    var minimumPos = -1;
    var candidates = GetCandidates(line, minimumPos);
    while(candidates.Count() > 0)
    {
        Console.WriteLine($"From pos: {minimumPos}");
        foreach(var entry in candidates) {
            Console.WriteLine($"Cand: {entry.Item1} @ {entry.Item2}");
        }
        line = line.Insert(candidates.First().Item2, candidates.First().Item1);
        minimumPos = candidates.First().Item2 + 2;
        Console.WriteLine(line);
        
        candidates = GetCandidates(line, minimumPos);
    }
    
    return line;
}

(char First, char Last)? GetFirstAndLastDigitsFromLine(string line) 
{
    var digits = line.Where(char.IsDigit).ToArray();    
    if(digits.Length == 0)
    {
        return null;
    }
    return (digits[0], digits[digits.Length - 1]);
}

int GetNumberFromDigits((char First, char Last)? digits) 
{
    if(digits.HasValue &&
        int.TryParse($"{digits.Value.First}{digits.Value.Last}", out int result))
    {
        return result;
    }
    return 0;
}
