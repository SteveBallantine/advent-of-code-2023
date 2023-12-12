var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\12-1\input.txt");

//var lines = new string[] { "#.#.### 1,1,3" }; // 1
//var lines = new string[] { ".??..??...?##. 1,1,3" }; // 4
//var lines = new string[] { "?#?#?#?#?#?#?#? 1,3,1,6" }; // 1
//var lines = new string[] { "?###???????? 3,2,1" }; // 10
//var lines = new string[] { "?????.?#???? 1,2,1,1" }; // 14
//var lines = new string[] { "???.##??##???#??.# 1,1,7,1,1,1" }; // 1
//var lines = new string[] { "???#??#.##. 2,1,2" }; // 2

var c = 0;
foreach (var line in lines)
{
    var parts = line.Split(' ');
    
    var finalBlockSizes = parts[1]
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToArray();
    
    var variations = GetVariations(parts[0], finalBlockSizes);
    Console.WriteLine($"{variations} for {line}");
    c += variations;
}
Console.WriteLine(c);

int GetVariations(string line, int[] finalBlockSizes)
{
    int[] startingPositions = GetStartingPositions(line, finalBlockSizes);
    var distanceToAffectBlocks = Enumerable.Range(0, startingPositions.Length)
        .ToDictionary(i => i, i => DistanceToAffectNextBlocks(i, finalBlockSizes, startingPositions).ToArray());
    Console.WriteLine($"Starting positions for {line} {string.Join(',', startingPositions)}");

    if (startingPositions[^1] == 0)
    {
        throw new Exception($"No matching positions for {line} {string.Join(',', finalBlockSizes)}");
    }

    return CountVariations(line, finalBlockSizes, startingPositions, distanceToAffectBlocks);
}

int CountVariations(string line, int[] finalBlockSizes, int[] startingPositions,
    Dictionary<int, int[]> connectedBlocks)
{
    return CountVariationsAtIndex(line, finalBlockSizes, startingPositions, connectedBlocks, 0, new int[startingPositions.Length]);
}

int CountVariationsAtIndex(string line, int[] finalBlockSizes, int[] startingPositions, Dictionary<int, int[]> distanceToAffectBlocks, int indexToVary, int[] variations)
{
    int count = 0;
    bool continueSearching = true;
    while (continueSearching)
    {
        continueSearching = false;

        var validity = IsVariationValid(line, finalBlockSizes, startingPositions, variations, indexToVary);
        switch (validity)
        {
            case Validity.Valid:
                int subCount = 0;
                if (indexToVary >= finalBlockSizes.Length - 1)
                {
                    subCount++;
                    //Console.WriteLine($"valid: {string.Join(',', Enumerable.Range(0, finalBlockSizes.Length).Select(i => startingPositions[i] + variations[i]))}");
                }
                else
                {
                    subCount += CountVariationsAtIndex(line, finalBlockSizes, startingPositions, distanceToAffectBlocks, indexToVary + 1, variations.Clone() as int[]);
                }
                count+=subCount;
                continueSearching = true;
                break;
            case Validity.InvalidButValidBeforeLimit:
                //Console.WriteLine($"Might have valid children: {string.Join(',', Enumerable.Range(0, finalBlockSizes.Length).Select(i => startingPositions[i] + variations[i]))}");
                if (indexToVary < finalBlockSizes.Length - 1)
                {
                    count += CountVariationsAtIndex(line, finalBlockSizes, startingPositions, distanceToAffectBlocks, indexToVary + 1, variations.Clone() as int[]);
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
        if (validity == Validity.Invalid && i > limit && result < Validity.InvalidButValidBeforeLimit)
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
    if (hashCoverageResult > result)
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
