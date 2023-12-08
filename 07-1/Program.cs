var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\07-1\input.txt");

var hands = lines.Select(GetHand).OrderBy(x => x.HandType)
    .ThenBy(x => x.GetCardRank(0))
    .ThenBy(x => x.GetCardRank(1))
    .ThenBy(x => x.GetCardRank(2))
    .ThenBy(x => x.GetCardRank(3))
    .ThenBy(x => x.GetCardRank(4))
    .ToArray();

for(var handRank = 1; handRank <= hands.Length; handRank++)
{
    var hand = hands[handRank - 1];
    hand.Winnings = handRank * hand.Bid;
    Console.WriteLine($"{hand.Cards} - {hand.HandType}-{hand.GetCardRank(0)} - {hand.Bid} * {handRank} = {hand.Winnings}");
}
Console.WriteLine(hands.Sum(x => x.Winnings));


Hand GetHand(string line)
{
    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (long.TryParse(parts[1], out long bid) == false)
    {
        throw new Exception("Fail");
    }
    return new Hand(parts[0], bid);
}

record Hand(string Cards, long Bid)
{
    private HandType? _type = null;
    public HandType HandType {
        get
        {
            if (_type == null)
            {
                _type = CalculateHandType();
            }
            return _type.Value;
        }
    }

    public long Winnings { get; set; }

    public int GetCardRank(int cardIndex)
    {
        return CardLabelToRank[Cards[cardIndex]];
    }

    HandType CalculateHandType()
    {
        var charGroups = Cards.GroupBy(x => x).ToArray();
        
        var groupCount = charGroups.Length;
        int largestGroup;
        var handHasNonJokers = charGroups.Any(g => g.Key != 'J');

        if (handHasNonJokers)
        {
            largestGroup = charGroups.Where(g => g.Key != 'J').Max(g => g.Count());
            if (Cards.Contains('J'))
            {
                groupCount--;
                largestGroup += charGroups.Single(g => g.Key == 'J').Count();
            }
        }
        else
        {
            largestGroup = 5;
        }

        if (groupCount == 1)
        {
            return HandType.FiveOfAKind;
        }
        if (largestGroup == 4)
        {
            return HandType.FourOfAKind;
        }
        if (largestGroup == 3)
        {
            if (groupCount == 2)
            {
                return HandType.FullHouse;
            }

            return HandType.ThreeOfAKind;
        }
        if (largestGroup == 2)
        {
            if (groupCount == 3)
            {
                return HandType.TwoPair;
            }

            return HandType.OnePair;
        }
        return HandType.HighCard;
    }
    

    Dictionary<char, int> CardLabelToRank = new Dictionary<char, int>()
    {
        { 'A', 13 },
        { 'K', 12 },
        { 'Q', 11 },
        { 'T', 10 },
        { '9', 9 },
        { '8', 8 },
        { '7', 7 },
        { '6', 6 },
        { '5', 5 },
        { '4', 4 },
        { '3', 3 },
        { '2', 2 },
        { 'J', 1 },
    };
}

enum HandType
{
    HighCard = 1,
    OnePair = 2,
    TwoPair = 3,
    ThreeOfAKind = 4,
    FullHouse = 5,
    FourOfAKind = 6,
    FiveOfAKind = 7,
};

