AssertFor(new [] { @"rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7" }, 1320);

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\15-1\input.txt");
Console.WriteLine(RunFor(lines, true));

long RunFor(string[] input, bool logging)
{
    long result = 0;

    var segments = input[0].Split(',');

    foreach (var segment in segments)
    {
        result += GetHash(segment);
    }
    return result;
}

int GetHash(string segment)
{
    int result = 0;
    foreach (var x in segment)
    {
        result += x;
        result *= 17;
        result %= 256;
    }
    return result;
}

void AssertFor(string[] input, long expectedResult)
{
    var result = RunFor(input, false);
    if (result != expectedResult)
    {
        foreach (var line in input)
        {
            Console.WriteLine(line);
        }
        throw new Exception($"Result was {result} but expected {expectedResult}");
    }
}