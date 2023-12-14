using System.Diagnostics;

AssertFor(new [] { "#.#.### 1,1,3" }, 1);
AssertFor(new [] { ".??..??...?##. 1,1,3" }, 4); 
AssertFor(new [] { "?#?#?#?#?#?#?#? 1,3,1,6" }, 1); 
AssertFor(new [] { "?###???????? 3,2,1" }, 10); 
AssertFor(new [] { "?????.?#???? 1,2,1,1" }, 14); 
AssertFor(new [] { "???.##??##???#??.# 1,1,7,1,1,1" }, 1); 
AssertFor(new [] { "???#??#.##. 2,1,2" }, 2); 
AssertFor(new []
    { ".??..??...?##.?.??..??...?##. 1,1,3,1,1,3" }, 32); 
AssertFor(new []
    { ".??..??...?##.?.??..??...?##.?.??..??...?##.?.??..??...?##.?.??..??...?##. 1,1,3,1,1,3,1,1,3,1,1,3,1,1,3" }, 16384);
AssertFor(new []
    { "?###??????????###??????????###???????? 3,2,1,3,2,1,3,2,1" }, 2250);
AssertFor(new []
    { "?###??????????###??????????###??????????###??????????###???????? 3,2,1,3,2,1,3,2,1,3,2,1,3,2,1" }, 506250);
AssertFor(new []
    { "?????.??????.?????????????.??????.?????????????.??????.?????????????.??????.?????????????.??????.??????? 3,2,1,1,1,2,3,2,1,1,1,2,3,2,1,1,1,2,3,2,1,1,1,2,3,2,1,1,1,2" }, 4831394944410);

var testInput = @"???.### 1,1,3
.??..??...?##. 1,1,3
?#?#?#?#?#?#?#? 1,3,1,6
????.#...#... 4,1,1
????.######..#####. 1,6,5
?###???????? 3,2,1";
var testLines = testInput.Split(System.Environment.NewLine);

AssertFor(testLines, 21); 
AssertFor(RepeatFiveTimes(testLines).ToArray(), 525152); 


var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\12-1\input.txt");
Console.WriteLine(RunFor(RepeatFiveTimes(lines).ToArray(), true));

void AssertFor(string[] lines, long expectedValue)
{
    var result = RunFor(lines, false);
    foreach (var line in lines)
    {
        Console.WriteLine(line);
    }
    Console.WriteLine($"Expected {expectedValue}. Actual {result}");
    if (result != expectedValue)
    {
        throw new Exception("Fail");
    }
}

IEnumerable<string> RepeatFiveTimes(string[] lines)
{
    foreach (var line in lines)
    {
        var parts = line.Split(' ');
        var tmp = new [] { parts[0], parts[0], parts[0], parts[0], parts[0] };
        var tmp2 = new [] { parts[1], parts[1], parts[1], parts[1], parts[1] };
        yield return string.Join('?', tmp) + " " + string.Join(',', tmp2);
    }
}

long RunFor(string[] lines, bool logging)
{
    long c = 0;
    foreach (var line in lines)
    {
        var variations = GetVariationsForLine(line, false, logging);
        c += variations;
        if (logging)
        {
            Console.WriteLine($"{variations} for {line}");
        }
    }
    return c;
}

long GetVariationsForLine(string line, bool verify, bool logging)
{
    var parts = line.Split(' ');
    var cache = new Dictionary<(int, int, bool), long>();
    
    var finalBlockSizes = parts[1]
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToArray();
    return GetVariations(parts[0], finalBlockSizes, cache, verify, logging);
}

long GetVariations(string line, int[] finalBlockSizes, Dictionary<(int, int, bool), long> cache, bool verify, bool logging)
{
    int[] startingPositions = GetStartingPositions(line, finalBlockSizes);
    var distanceToAffectBlocks = Enumerable.Range(0, startingPositions.Length)
        .ToDictionary(i => i, i => DistanceToAffectNextBlocks(i, finalBlockSizes, startingPositions).ToArray());
    //Console.WriteLine($"Starting positions for {line} {string.Join(',', startingPositions)}");

    if (startingPositions.Length > 1 && startingPositions[^1] == 0)
    {
        throw new Exception($"No matching positions for {line} {string.Join(',', finalBlockSizes)}");
    }

    return CountVariations(line, finalBlockSizes, startingPositions, distanceToAffectBlocks, cache, verify, logging);
}

long CountVariations(string line, int[] finalBlockSizes, int[] startingPositions,
    Dictionary<int, int[]> connectedBlocks, Dictionary<(int, int, bool), long> cache, bool verify, bool logging)
{
    return CountVariationsAtIndex(line, finalBlockSizes, startingPositions, connectedBlocks, 0, new int[startingPositions.Length], cache, verify, logging);
}

long CountVariationsAtIndex(string line, int[] finalBlockSizes, int[] startingPositions, Dictionary<int, int[]> distanceToAffectBlocks, int indexToVary, int[] variations, Dictionary<(int, int, bool), long> cache, bool verify, bool logging)
{
    long count = 0;
    bool continueSearching = true;
    while (continueSearching)
    {
        continueSearching = false;
        var validity = IsVariationValid(line, finalBlockSizes, startingPositions, variations, indexToVary);

        switch (validity)
        {
            case Validity.Valid:
                long subCount = 0;
                if (indexToVary >= finalBlockSizes.Length - 1)
                {
                    subCount++;
                    //Console.WriteLine($"valid: {string.Join(',', Enumerable.Range(0, finalBlockSizes.Length).Select(i => startingPositions[i] + variations[i]))}");
                }
                else
                {
                    var key = (indexToVary + 1, variations[indexToVary + 1], true);
                    if (cache.TryGetValue(key, out long v))
                    {
                        //Console.WriteLine($"cache hit: {key} = {v} for {string.Join(',', Enumerable.Range(0, finalBlockSizes.Length).Select(i => startingPositions[i] + variations[i]))}");
                        subCount += v;                      
                        if (verify)
                        {
                            Verify(v, line, finalBlockSizes, startingPositions, indexToVary + 1, variations);
                        }
                    }
                    else
                    {
                        var vCount = CountVariationsAtIndex(line, finalBlockSizes, startingPositions,
                            distanceToAffectBlocks, indexToVary + 1, variations.Clone() as int[], cache, verify, logging);
                        //Console.WriteLine($"Add cache entry: {key} = {vCount} for {string.Join(',', Enumerable.Range(0, finalBlockSizes.Length).Select(i => startingPositions[i] + variations[i]))} => {GetSubLineForPosition(line, finalBlockSizes, startingPositions, indexToVary, variations)}");
                        cache.Add(key, vCount);
                        subCount += vCount;
                    }
                }
                count+=subCount;
                continueSearching = true;
                break;
            case Validity.InvalidButValidBeforeLimit:
                //Console.WriteLine($"Might have valid children: {string.Join(',', Enumerable.Range(0, finalBlockSizes.Length).Select(i => startingPositions[i] + variations[i]))}");
                if (indexToVary < finalBlockSizes.Length - 1)
                {
                    var nextLevelIsValid = IsVariationValid(line, finalBlockSizes, startingPositions, variations,
                        indexToVary + 1);
                    var useCache = nextLevelIsValid == Validity.Valid || nextLevelIsValid == Validity.InvalidButValidBeforeLimit;

                    var key = (indexToVary + 1, variations[indexToVary + 1], false);
                    if (useCache && cache.TryGetValue(key, out long v))
                    {                            
                        if (verify)
                        {
                            Verify(v, line, finalBlockSizes, startingPositions, indexToVary + 1, variations);
                        }
                        if (v > 0)
                        {
                            //Console.WriteLine($"cache hit: {key} = {v} for {string.Join(',', Enumerable.Range(0, finalBlockSizes.Length).Select(i => startingPositions[i] + variations[i]))}");
                        }

                        count += v;
                    }
                    else
                    {
                        var vCount = CountVariationsAtIndex(line, finalBlockSizes, startingPositions,
                            distanceToAffectBlocks, indexToVary + 1, variations.Clone() as int[], cache, verify, logging);

                        if (useCache)
                        {
                            cache.Add(key, vCount);
                            //Console.WriteLine($"Add cache entry: {key} = {vCount} for {string.Join(',', Enumerable.Range(0, finalBlockSizes.Length).Select(i => startingPositions[i] + variations[i]))} => {GetSubLineForPosition(line, finalBlockSizes, startingPositions, indexToVary, variations)}");
                        }
                        count += vCount;
                    }
                }
                continueSearching = true;
                break;
            case Validity.Invalid:
                continueSearching = true;
                break;
            case Validity.InvalidAndNoFurtherVariationCanPossiblyBeValid:
            case Validity.InvalidWithHashLeftBehind:
                //Console.WriteLine($"Dead end: {string.Join(',', Enumerable.Range(0, finalBlockSizes.Length).Select(i => startingPositions[i] + variations[i]))}");
                break;
        }
        
        variations[indexToVary]++;
        
        // Do we have to shift other blocks?
        for(int i = indexToVary + 1; i < finalBlockSizes.Length; i++)
        {
            var distanceToAffectBlock = distanceToAffectBlocks[indexToVary][i - indexToVary - 1];
            if (variations[indexToVary] >= distanceToAffectBlock)
            {
                variations[i]++;
            }
        }
    }

    return count;
}

string GetSubLineForPosition(string line, int[] finalBlockSizes, int[] startingPositions,
    int indexToVary, int[] variations)
{
    int start = startingPositions[indexToVary] + variations[indexToVary];
    string testLine = line.Substring(start, line.Length - start);
    int[] testBlocks = finalBlockSizes.Skip(indexToVary).ToArray();
    return $"{testLine} {string.Join(',', testBlocks)}";
}

void Verify(long calculatedValue, string line, int[] finalBlockSizes, int[] startingPositions, int indexToVary, int[] variations)
{
    if (calculatedValue > 0)
    {
        var subLine = GetSubLineForPosition(line, finalBlockSizes, startingPositions, indexToVary, variations);
        var subResult = GetVariationsForLine(subLine, false, false);
        Debug.Assert(subResult == calculatedValue);
        //Console.WriteLine($"Verified {subResult} = {calculatedValue}");
    }
}

IEnumerable<int> DistanceToAffectNextBlocks(int index, int[] finalBlockSizes, int[] startingPositions)
{
    int maxConnectedDistanceFromIndex = finalBlockSizes[index] + 1;
    for (int i = index + 1; i < finalBlockSizes.Length; i++)
    {
        var distanceBeforeConnection = startingPositions[i] - (startingPositions[index] + maxConnectedDistanceFromIndex) + 1;
        yield return distanceBeforeConnection;
        maxConnectedDistanceFromIndex += finalBlockSizes[i] + 1;
    }
}


Validity IsVariationValid(string line, int[] finalBlockSizes, int[] startingPositions, int[] variations, int limit)
{
    var result = Validity.Valid;
    for (int i = 0; i < finalBlockSizes.Length; i++)
    {
        var validity = IsValidAtPosition(line, startingPositions[i] + variations[i], finalBlockSizes[i]);
        if (validity == Validity.Invalid && i > limit && result == Validity.Valid)
        {
            result = Validity.InvalidButValidBeforeLimit;
        }
        if (result != Validity.InvalidButValidBeforeLimit && validity == Validity.InvalidWithHashLeftBehind && i > limit)
        {
            result = Validity.Invalid;
        } 
        else if (validity > result)
        {        
            result = validity;
        }
    }

    var hashCoverageResult = AllHashesCovered(line, finalBlockSizes,
        Enumerable.Range(0, finalBlockSizes.Length).Select(i => startingPositions[i] + variations[i]).ToArray(), limit);
    if ((hashCoverageResult > result || 
        hashCoverageResult == Validity.Invalid && result == Validity.InvalidButValidBeforeLimit) &&
        !(hashCoverageResult == Validity.InvalidButValidBeforeLimit && result == Validity.Invalid))
    {
        result = hashCoverageResult;
    }
    
    return result;
}

Validity AllHashesCovered(string line, int[] finalBlockSizes, int[] positions, int limit)
{
    int b = 0;
    for (int i = 0; i < line.Length; i++)
    {
        if (line[i] == '#')
        {
            if (b >= finalBlockSizes.Length ||
                i < positions[b])
            {
                return b > limit ? Validity.InvalidButValidBeforeLimit : Validity.Invalid;
            }
        }

        if (b < finalBlockSizes.Length &&
            i >= positions[b] + finalBlockSizes[b]) b++;
    }
    return Validity.Valid;
}


int[] GetStartingPositions(string line, int[] finalBlockSizes)
{
    int nextBlock = 0;
    int[] blockPositions = new int[finalBlockSizes.Length];

    // Want to check if all '#' are covered as well.
    int i = 0;
    while(i < line.Length &&
          nextBlock < finalBlockSizes.Length)
    {
        if (IsValidAtPosition(line, i, finalBlockSizes[nextBlock]) == Validity.Valid)
        {
            blockPositions[nextBlock] = i;
            bool nextCharIsSpace = i < line.Length - 1 && line[i + 1] == '.';
            i += finalBlockSizes[nextBlock] - (nextCharIsSpace ? 1 : 0);
            nextBlock++;
        }

        i++;
    }

    return blockPositions;
}

Validity IsValidAtPosition(string line, int position, int blockSize)
{
    if (position >= line.Length) return Validity.InvalidAndNoFurtherVariationCanPossiblyBeValid;
    int i = position;
    // If previous char is '#' then the position cannot be valid.
    if (i > 0 && line[i - 1] == '#') return Validity.InvalidWithHashLeftBehind;
    
    while(i - position < blockSize)
    {
        if (i >= line.Length) return Validity.InvalidAndNoFurtherVariationCanPossiblyBeValid;
        if (line[i] == '.') return Validity.Invalid;
        i++;
    }
    // If next char after this block is another '#' then the position is not valid.
    if (i < line.Length && line[i] == '#') return Validity.Invalid;
    
    return Validity.Valid;
}

enum Validity
{
    Valid,
    Invalid,
    InvalidWithHashLeftBehind,
    InvalidButValidBeforeLimit,
    InvalidAndNoFurtherVariationCanPossiblyBeValid
}
