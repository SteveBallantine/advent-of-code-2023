
/*AssertFor(@"broadcaster -> a, b, c
%a -> b
%b -> c
%c -> inv
&inv -> a", 32000000);

AssertFor(@"broadcaster -> a
%a -> inv, con
&inv -> b
%b -> con
&con -> output", 11687500);*/

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\20-1\input.txt");
Console.WriteLine(RunFor(lines, true));

long RunFor(string[] input, bool logging)
{
    var set = new ModuleSet();
    set.Parse(input);

    /*(int Low, int High) result = (0, 0);
    for (int i = 0; i < 1000; i++)
    {
        var thisTime = set.PushButton();
        result.High += thisTime.High;
        result.Low += thisTime.Low;
    }*/
    
    var presses = set.PushButtonUntilRx();

    Console.WriteLine(presses);
    return presses;
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


public class ModuleSet
{
    private ModuleBase[] _modules;
    private ButtonModule _button;
    private Queue<Pulse> _pulseQueue = new ();

    public long PushButtonUntilRx()
    {
        long result = 0;
        bool rxSignaled = false;
        while (!rxSignaled)
        {
            _button.ProcessPulse(null, false);
            result++;

            while (_pulseQueue.TryDequeue(out var pulse))
            {
                //Console.WriteLine($"{pulse.Sender.Label} - {(pulse.High ? "high" : "low")} -> {pulse.Reciever.Label}");
                if (pulse.Reciever.Label == "dn" && pulse.High)
                {
                    Console.WriteLine($"dn received high pulse from {pulse.Sender.Label} after {result} presses");
                }
                if (pulse.Reciever.Label == "rx" && !pulse.High)
                {
                    rxSignaled = true;
                    break;
                }
                pulse.Reciever.ProcessPulse(pulse.Sender, pulse.High);
            }

            if (result % 1000000 == 0)
            {
                Console.WriteLine(result);
            }
        }

        return result;
    }
    
    public (int Low, int High) PushButton()
    {
        _button.ProcessPulse(null, false);
        (int Low, int High) result = (0, 0);

        while (_pulseQueue.TryDequeue(out var pulse))
        {
            //Console.WriteLine($"{pulse.Sender.Label} - {(pulse.High ? "high" : "low")} -> {pulse.Reciever.Label}");
            if(pulse.High)
            {
                result.High++;
            }
            else
            {
                result.Low++;
            }
            pulse.Reciever.ProcessPulse(pulse.Sender, pulse.High);
        }
        
        //Console.WriteLine(result);
        return result;
    }
    
    public void Parse(string[] input)
    {
        var modulesByLabel = CreateModules(input);

        foreach (var line in input)
        {
            var parts = line.Split("->", StringSplitOptions.TrimEntries);
            var label = GetLabel(parts[0]);
            var destinationLabels = parts[1].Split(',', StringSplitOptions.TrimEntries);

            var destinations = new List<ModuleBase>();
            foreach (var destination in destinationLabels)
            {
                if (modulesByLabel.TryGetValue(destination, out var destModule))
                {
                    destinations.Add(destModule);
                }
                else
                {
                    destinations.Add(new NullModule(destination));
                }
            }

            modulesByLabel[label].DestinationModules = destinations.ToArray();
        }
        
        _modules = modulesByLabel.Select(x => x.Value).ToArray();
        
        _modules.OfType<ConjunctionModule>().ToList().ForEach(x => x.Init(GetSendersTo(x)));
        
        CreateButton();
    }

    private void CreateButton()
    {
        var button = new ButtonModule("push-button", _pulseQueue);
        button.DestinationModules = new [] { _modules.Single(m => m.GetType() == typeof(BroadcasterModule)) };
        _button = button;
    }

    private Dictionary<string, ModuleBase> CreateModules(string[] input)
    {
        var modulesByLabel = new Dictionary<string, ModuleBase>();
        
        foreach (var line in input)
        {
            var parts = line.Split("->", StringSplitOptions.TrimEntries);

            var label = GetLabel(parts[0]);
            if (label == "broadcaster")
            {
                modulesByLabel.Add(label, new BroadcasterModule(parts[0], _pulseQueue));
            }
            else
            {
                var typeChar = parts[0][0];
                ModuleBase newModule;
                switch (typeChar)
                {
                    case '%':
                        newModule = new FlipFlopModule(label, _pulseQueue);
                        break;
                    case '&':
                        newModule = new ConjunctionModule(label, _pulseQueue);
                        break;
                    default:
                        throw new Exception("Bad");
                }
                
                modulesByLabel.Add(label, newModule);
            }
        }

        return modulesByLabel;
    }

    private ModuleBase[] GetSendersTo(ModuleBase module)
    {
        return _modules.Where(m => m.DestinationModules.Contains(module)).ToArray();
    } 

    private string GetLabel(string trimmedLabelText)
    {
        if (trimmedLabelText == "broadcaster")
        {
            return trimmedLabelText;
        }
        return trimmedLabelText.Substring(1);
    }
}

abstract class ModuleBase
{
    public string Label { get; init; }
    public ModuleBase[] DestinationModules { get; set; }

    public abstract void ProcessPulse(ModuleBase sender, bool high);
}

record Pulse(ModuleBase Sender, ModuleBase Reciever, bool High);

class FlipFlopModule : ModuleBase
{
    private bool _on = false;
    private Queue<Pulse> _pulseQueue;
    
    public FlipFlopModule(string label, Queue<Pulse> pulseQueue)
    {
        Label = label;
        _pulseQueue = pulseQueue;
    }

    public override void ProcessPulse(ModuleBase sender, bool high)
    {
        if (!high)
        {
            _on = !_on;
            foreach (var module in DestinationModules)
            {
                _pulseQueue.Enqueue(new Pulse(this, module, _on));
            }
        }
    }
}

class ConjunctionModule : ModuleBase
{
    private Dictionary<ModuleBase, bool> _lastPulseHighByModule;
    private Queue<Pulse> _pulseQueue;
    
    public ConjunctionModule(string label, Queue<Pulse> pulseQueue)
    {
        Label = label;
        _pulseQueue = pulseQueue;
    }

    public void Init(ModuleBase[] possibleSenders)
    {
        _lastPulseHighByModule = possibleSenders.ToDictionary(x => x, x => false);
    }

    public override void ProcessPulse(ModuleBase sender, bool high)
    {
        _lastPulseHighByModule[sender] = high;
        bool sendHigh = !_lastPulseHighByModule.All(x => x.Value);
        foreach (var module in DestinationModules)
        {
            _pulseQueue.Enqueue(new Pulse(this, module, sendHigh));
        }
    }
}

class BroadcasterModule : ModuleBase
{
    private Queue<Pulse> _pulseQueue;
    
    public BroadcasterModule(string label, Queue<Pulse> pulseQueue)
    {
        Label = label;
        _pulseQueue = pulseQueue;
    }

    public override void ProcessPulse(ModuleBase sender, bool high)
    {
        foreach (var module in DestinationModules)
        {
            _pulseQueue.Enqueue(new Pulse(this, module, high));
        }
    }
}

class ButtonModule : ModuleBase
{
    private Queue<Pulse> _pulseQueue;
    
    public ButtonModule(string label, Queue<Pulse> pulseQueue)
    {
        Label = label;
        _pulseQueue = pulseQueue;
    }

    public override void ProcessPulse(ModuleBase sender, bool high)
    {
        foreach (var module in DestinationModules)
        {
            _pulseQueue.Enqueue(new Pulse(this, module, false));
        }
    }
}

class NullModule : ModuleBase
{
    public NullModule(string label)
    {
        Label = label;
    }

    public override void ProcessPulse(ModuleBase sender, bool high)
    {
    }
}