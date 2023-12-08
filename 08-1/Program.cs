var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\08-1\input.txt");

var blocks = SplitIntoBlocks(lines).ToList();
if (blocks.Count > 2) throw new Exception("Too many blocks");

var instructions = blocks.First().Single();
var graph = new Graph(GetNodes(blocks[1]).ToArray());

Console.WriteLine(GetNumberOfMoves());

long GetNumberOfMoves()
{
    string currentLocation = "AAA";
    long moves = 0;
    while (currentLocation != "ZZZ")
    {
        var index = (int)moves % instructions.Length;
        var move = instructions[index];
        Console.Write($"{currentLocation} go {move} to ");
        currentLocation = graph.GetNextLocation(currentLocation, move);
        moves++;
        Console.WriteLine(currentLocation);
    }

    return moves;
}

IEnumerable<Node> GetNodes(string[] lines)
{
    foreach (var line in lines)
    {
        var parts = line.Split(new char[] { '=', '(', ')', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 3) throw new Exception($"Too many parts {string.Join(",", parts)}");
        yield return new Node(parts[0], parts[1], parts[2]);
    }
}

IEnumerable<string[]> SplitIntoBlocks(string[] lines)
{
    List<string> result = new List<string>();
    for (int i = 0; i < lines.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(lines[i]))
        {
            yield return result.ToArray();
            result.Clear();
        }
        else
        {
            result.Add(lines[i]);
        }
    }
    yield return result.ToArray();
}

class Graph
{
    private HashSet<char> _validMoves = new HashSet<char>() { 'L', 'R' };
    private Node[] _nodes;
    private Dictionary<string, Node> _lookup;
    
    public Graph(Node[] nodes)
    {
        _nodes = nodes;
        _lookup = _nodes.ToDictionary(n => n.Label, n => n);
    }

    public string GetNextLocation(string currentLocation, char move)
    {
        if (!_validMoves.Contains(move)) throw new Exception($"Invalid move {move}");
        if (!_lookup.TryGetValue(currentLocation, out var node)) throw new Exception($"cannot find node {currentLocation}");
        return move == 'L' ? node.Left : node.Right;
    }
}

record Node(string Label, string Left, string Right);