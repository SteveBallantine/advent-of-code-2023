var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\04-1\input.txt");

DoPart2(lines);


void DoPart2(string[] lines)
{
    var cards = lines.Select(GetCard).ToArray();
    UpdateInstanceCounts(cards);

    foreach (var card in cards)
    {
        Console.WriteLine(card.InstanceCount);
    }
    Console.WriteLine(cards.Sum(c => c.InstanceCount));
}


void DoPart1(string[] lines)
{
    var result = lines
        .Select(GetCard)
        .Select(x => x.GetScore())
        .Sum();

    Console.WriteLine(result);
}


void UpdateInstanceCounts(Card[] cards)
{
    for(int i = 0; i < cards.Length; i++)
    {
        var cardsToDuplicate = cards[i].GetMyWinners().Length;
        for (int j = 1; j < cardsToDuplicate + 1; j++)
        {
            cards[i + j].InstanceCount += cards[i].InstanceCount;
        }
    }
}

Card GetCard(string line)
{
    var startRemoved = line.Substring(line.IndexOf(':') + 1);
    Console.WriteLine(startRemoved);
    var parts = startRemoved.Split('|');
    var card = new Card(Parse(parts[0]), Parse(parts[1]));
    card.WriteToConsole();
    return card;
}

int[] Parse(string numbers)
{
    return numbers.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(x =>
        {
            return int.Parse(new string(x.Where(c => char.IsDigit(c)).ToArray()));
        })
        .ToArray();
}

record Card(int[] Winners, int[] MyNumbers)
{
    public int InstanceCount { get; set; } = 1;

    public int[] GetMyWinners()
    {
        return MyNumbers.Where(x => Winners.Contains(x)).ToArray();
    }

    public int GetScore()
    {
        var numWinners = GetMyWinners().Length;
        int result = numWinners >= 1 ? 1 : 0;
        for (int i = 1; i < numWinners; i++)
        {
            result *= 2;
        }
        return result;
    }

    
    public void WriteToConsole()
    {
        foreach (var n in Winners)
        {
            Console.Write(n);
            Console.Write(' ');
        }
        Console.Write('|');
        Console.Write(' ');
        foreach (var n in MyNumbers)
        {
            if (Winners.Contains(n))
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.Write(n); 
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(' ');
        }

        Console.Write('*');
        Console.Write(' ');
        Console.ForegroundColor = ConsoleColor.Blue;
        foreach (var entry in GetMyWinners())
        {
            Console.Write(entry);
            Console.Write(' ');
        }
        //WritePart1Score();
        WritePart2Score();
        Console.ForegroundColor = ConsoleColor.Gray;
        
        Console.WriteLine();
    }

    private void WritePart1Score()
    {
        Console.Write($"* {GetMyWinners().Length} winners = {GetScore()} points");
    }

    private void WritePart2Score()
    {
        
    }
}
