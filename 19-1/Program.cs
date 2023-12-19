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
{x=2127,m=1623,a=2188,s=1013}", 167409079868000);

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
    
    return GetCombos(workflows.ToArray());
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

long GetCombos(Workflow[] workflows)
{
    var result = 0l;
    var paths = GetPathsToAcceptance(workflows).ToArray();

    List<Dictionary<string, Range>> countedRanges = new List<Dictionary<string, Range>>();
    foreach (var path in paths)
    {
        var valueRanges = GetValidValueRanges(path);
        long combos = CountCombos(valueRanges);
        //combos -= GetDuplicateCombos(valueRanges, completed);
        countedRanges.Add(valueRanges);

        result += combos;
    }
    
    return result;
}

long CountCombos(Dictionary<string, Range> ranges)
{
    var diffX = (long)(ranges["x"].Max - ranges["x"].Min) + 1;
    var diffM = (long)(ranges["m"].Max - ranges["m"].Min) + 1;
    var diffA = (long)(ranges["a"].Max - ranges["a"].Min) + 1;
    var diffS = (long)(ranges["s"].Max - ranges["s"].Min) + 1;
    if (diffX < 0 || diffM < 0 || diffA < 0 || diffS < 0) return 0;
    return diffX * diffM * diffA * diffS;
}

Dictionary<string, Range> GetValidValueRanges(Filter[] path)
{
    var validRanges = new Dictionary<string, Range>();
    validRanges.Add("x", new Range { Min = 1, Max = 4000 });
    validRanges.Add("m", new Range { Min = 1, Max = 4000 });
    validRanges.Add("a", new Range { Min = 1, Max = 4000 });
    validRanges.Add("s", new Range { Min = 1, Max = 4000 });
    
    foreach (var entry in path)
    {
        if (entry is { Value: not null, Gt: not null })
        {
            if (entry.Gt.Value && validRanges[entry.PropertyName].Min < entry.Value.Value + 1)
            {
                validRanges[entry.PropertyName].Min = entry.Value.Value + 1;
            }
            else if (!entry.Gt.Value && validRanges[entry.PropertyName].Max > entry.Value.Value - 1)
            {
                validRanges[entry.PropertyName].Max = entry.Value.Value - 1;
            }
        }
    }

    return validRanges;
}

IEnumerable<Filter[]> GetPathsToAcceptance(Workflow[] workflows)
{
    var acceptPoints = FindAllPointsFor("A", workflows);
    
    foreach (var acceptPoint in acceptPoints)
    {
        Stack<(Workflow, int, Filter[], bool)> stateStack = new Stack<(Workflow, int, Filter[], bool)>();
        stateStack.Push((acceptPoint.Item1, acceptPoint.Item2, Array.Empty<Filter>(), false));
        bool start = true;
        
        while (stateStack.Count > 0)
        {
            var position = stateStack.Pop();

            var positionFilter = position.Item1.Filters[position.Item2];
            if (!start &&
                !position.Item4)
            {
                positionFilter = Filter.Reverse(positionFilter);
            }
            start = false;

            var nextFilters = position.Item3.Append(positionFilter).ToArray();
            
            if (position.Item1.Label == "in" && position.Item2 == 0)
            {
                yield return nextFilters;
            }
            
            if (position.Item2 > 0)
            {
                stateStack.Push((position.Item1, position.Item2 - 1, nextFilters, false));
            }
            else
            {
                var points = FindAllPointsFor(position.Item1.Label, workflows);
                foreach (var entry in points)
                {
                    stateStack.Push((entry.Item1, entry.Item2, (Filter[])nextFilters.Clone(), true));
                }
            }
        }
    }
}

List<(Workflow, int)> FindAllPointsFor(string label, Workflow[] workflows)
{
    List<(Workflow, int)> points = new List<(Workflow, int)>();
    foreach (var workflow in workflows)
    {
        for (int i = 0; i < workflow.Filters.Length; i++)
        {
            if (workflow.Filters[i].Destination == label)
            {
                points.Add((workflow, i));
            }
        }
    }

    return points;
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

record Range
{
    public int Min { get; set; }
    public int Max { get; set; }
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

    public static Filter Reverse(Filter filter)
    {
        return new Filter(T => filter.Gt.Value ? T[filter.PropertyName] < filter.Value + 1 : T[filter.PropertyName] > filter.Value - 1,
            filter.PropertyName, !filter.Gt, filter.Gt.Value ? filter.Value + 1 : filter.Value - 1, ">A");
    }
};