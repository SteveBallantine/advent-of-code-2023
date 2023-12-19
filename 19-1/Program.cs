using System.Dynamic;

AssertFor(@"px{a<2006:qkq,m>2090:A,rfg}
pv{a>1716:R,A}
lnx{m>1548:A,A}
rfg{s<537:gd,x>2440:R,A}
qs{s>3448:A,lnx}
qkq{x<1416:A,crn}
crn{x>2662:A,R}
in{s<1351:px,qqz}
qqz{s>2770:qs,m<1801:hdj,R}
gd{a>3333:R,R}
hdj{m>838:A,pv}

{x=787,m=2655,a=1222,s=2876}
{x=1679,m=44,a=2067,s=496}
{x=2036,m=264,a=79,s=2244}
{x=2461,m=1339,a=466,s=291}
{x=2127,m=1623,a=2188,s=1013}", 19114);

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\19-1\input.txt");
Console.WriteLine(RunFor(lines, true));

long RunFor(string[] input, bool logging)
{
    var blocks = BreakByEmptyLine(input).ToArray();

    List<Workflow> workflows = new();
    foreach (var entry in blocks[0])
    {
        workflows.Add(Workflow.Parse(entry));
    }
    
    List<Dictionary<string, int>> toys = new();
    foreach (var entry in blocks[1])
    {
        toys.Add(ParseToy(entry));
    }

    int result = 0;
    foreach (var toy in GetAcceptedToys(workflows.ToArray(), toys.ToArray()))
    {
        foreach (var property in toy)
        {
            result += property.Value;
        }
    }
    
    return result;
}

void AssertFor(string input, long expectedResult)
{
    var lines = input.Split(System.Environment.NewLine);
    var result = RunFor(lines, false);
    if (result != expectedResult)
    {
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
        throw new Exception($"Result was {result} but expected {expectedResult}");
    }
}

IEnumerable<Dictionary<string, int>> GetAcceptedToys(Workflow[] rawWorkflows, Dictionary<string, int>[] toys)
{
    var workflows = rawWorkflows.ToDictionary(w => w.Label, w => w);

    foreach (var toy in toys)
    {
        string nextStop = "in";
        while (nextStop != "R" && nextStop != "A")
        {
            nextStop = workflows[nextStop].GetNext(toy);
        }

        if (nextStop == "A")
        {
            yield return toy;
        }
    }
}

IEnumerable<string[]> BreakByEmptyLine(string[] data)
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

Dictionary<string, int> ParseToy(string input)
{
    var parts = input.Split(new [] { ',', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
    var result = new Dictionary<string, int>();
    foreach (var entry in parts)
    {
        var properyParts = entry.Split('=');
        result[properyParts[0]] = int.Parse(properyParts[1]);
    }
    return result;
}

record Workflow(string Label, Filter[] Filters)
{
    public string GetNext(dynamic toy)
    {
        foreach (var filter in Filters)
        {
            if (filter.Function(toy))
            {
                return filter.Destination;
            }
        }
        return "ZZZ";
    }
    
    public static Workflow Parse(string input)
    {
        var parts = input.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
        var filterStrings = parts[1].Split(',');
        return new Workflow(parts[0], filterStrings.Select(s => Filter.Parse(s)).ToArray());
    }
}

record Filter(Func<Dictionary<string, int>, bool> Function, string PropertyName, bool? Gt, int? Value, string Destination)
{
    public static Filter Parse(string input)
    {
        var parts = input.Split(':');
        if (parts.Length == 1)
        {
            return new Filter(_ => true, null, null, null, input);
        }
        
        var propertyName = parts[0].Substring(0, 1);
        var gT = parts[0][1] == '>';
        var value = int.Parse(parts[0].Substring(2));

        return new Filter(
            T => gT ? T[propertyName] > value : T[propertyName] < value,
            propertyName, gT, value, parts[1]);
    }
};