var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\08-1\input.txt");

var blocks = SplitIntoBlocks(lines).ToList();
if (blocks.Count > 2) throw new Exception("Too many blocks");

var instructions = blocks.First().Single();
var graph = new Graph(GetNodes(blocks[1]).ToArray());

var startNodes = graph.GetStartNodes();

List<Cycle> cycles = new List<Cycle>();
Parallel.ForEach(startNodes, startNode =>
{
    Console.WriteLine($"Searching for cycle from {startNode}");
    var cycle = FindCycle(startNode);
    Console.WriteLine($"Found cycle for start node {startNode} starting after {cycle.StepsBeforeCycle} with {cycle.StepsInCycle} steps. Exits at {string.Join(",", cycle.StepsToEndNodesInCycle)}");
    lock (cycles)
    {
        cycles.Add(cycle);
    }
});

var states = cycles.Select(x => new State(x)).ToArray();
var minDistance = states.Max(s => s.StepsToNextExit);

while (states.Any(s => s.StepsToNextExit < minDistance))
{
    foreach (var state in states)
    {
        while (state.StepsToNextExit < minDistance)
        {
            state.Advance();
        }
    }
    minDistance = states.Max(s => s.StepsToNextExit);
}

Console.WriteLine(states[0].StepsToNextExit);


Cycle FindCycle(string start)
{
    HashSet<MovementHistory> history = new HashSet<MovementHistory>();
    MovementHistory currentState = new MovementHistory(0, start);
    
    while (!history.Contains(currentState))
    {
        history.Add(currentState);
        var move = instructions[currentState.InstructionIndex];
        var nextIndex = currentState.InstructionIndex == instructions.Length - 1
            ? 0
            : currentState.InstructionIndex + 1; 
        currentState = new MovementHistory(nextIndex, graph.GetNextLocation(currentState.Location, move));
    }
    
    var stepsBeforeCycle = 0;
    while(history.ElementAt(stepsBeforeCycle) != currentState)
    {
        stepsBeforeCycle++;
    }

    return new Cycle(start, stepsBeforeCycle, history.Count - stepsBeforeCycle, GetStepsToEndNodes(history, stepsBeforeCycle).ToArray());
}

IEnumerable<int> GetStepsToEndNodes(HashSet<MovementHistory> history, int stepsBeforeCycle)
{
    for (int i = stepsBeforeCycle; i < history.Count; i++)
    {
        if (history.ElementAt(i).Location.EndsWith('Z'))
        {
            yield return i - stepsBeforeCycle;
        }
    }
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

class State
{
    private readonly Cycle _cycle;

    public State(Cycle cycle)
    {
        _cycle = cycle;
        StepsToNextExit = _cycle.StepsBeforeCycle + _cycle.StepsToEndNodesInCycle[0];
        ExitIndexInCurrentCycle = 0;
    }

    public void Advance()
    {
        if (_cycle.StepsToEndNodesInCycle.Length == 1)
        {
            StepsToNextExit += _cycle.StepsInCycle;
        }
        else
        {
            ExitIndexInCurrentCycle++;
            if (ExitIndexInCurrentCycle > _cycle.StepsToEndNodesInCycle.Length)
            {
                ExitIndexInCurrentCycle = 0;
                StepsToNextExit += _cycle.StepsInCycle -
                                   _cycle.StepsToEndNodesInCycle[^1] +
                                   _cycle.StepsToEndNodesInCycle[0];
            }
            else
            {
                StepsToNextExit += _cycle.StepsToEndNodesInCycle[ExitIndexInCurrentCycle] -
                                  _cycle.StepsToEndNodesInCycle[ExitIndexInCurrentCycle - 1];
            }
        }
    }

    public long StepsToNextExit { get; private set; }
    private long ExitIndexInCurrentCycle { get; set; }
};

record Cycle(string StartNode, int StepsBeforeCycle, int StepsInCycle, int[] StepsToEndNodesInCycle);

record MovementHistory(int InstructionIndex, string Location);

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

    public string[] GetStartNodes()
    {
        return _nodes.Where(n => n.Label.EndsWith('A')).Select(n => n.Label).ToArray();
    }
}

record Node(string Label, string Left, string Right);