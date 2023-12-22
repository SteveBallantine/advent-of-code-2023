AssertFor(@"1,0,1~1,2,1
0,0,2~2,0,2
0,2,3~2,2,3
0,0,4~0,2,4
2,0,5~2,2,5
0,1,6~2,1,6
1,1,8~1,1,9", 5);

var lines = File.ReadAllLines(@"C:\Repos\advent-of-code-2023\22-1\input.txt");
Console.WriteLine(RunFor(lines, true));

long RunFor(string[] input, bool logging)
{
    var tower = GetTower(input);
    tower.SettlePile();
    return tower.CountRemovable();
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

SandTower GetTower(string[] input)
{
    return new SandTower(ParseToBlocks(input).ToArray());
}

IEnumerable<Block> ParseToBlocks(string[] input)
{
    var label1 = 'A';
    var label2 = 'A';
    foreach (var line in input)
    {
        yield return ParseToBlock(line, new string(new [] {label1, label2}));
        
        label2++;
        if (label2 > 'Z')
        {
            label1++;
            label2 = 'A';
        }
    }
}

Block ParseToBlock(string input, string label)
{
    var coords = input.Split("~").Select(x => x.Split(',').Select(int.Parse).ToArray()).ToArray();

    return new Block(label, new Point(coords.Min(x => x[0]), coords.Min(x => x[1]), coords.Min(x => x[2])),
        new Point(coords.Max(x => x[0]), coords.Max(x => x[1]), coords.Max(x => x[2])));
}

class Block
{
    public string Label { get; init; }
    public Point2D MinXY { get; init; }
    public Point2D MaxXY { get; init; }
    public int Width { get; init; }
    public int Depth { get; init; }
    public int Height { get; init; }
    
    public int HeightFromGroundAtBase { get; set; }
    
    public Block(string label, Point minXYZ, Point maxXYZ)
    {
        Label = label;
        MinXY = new Point2D(minXYZ.X, minXYZ.Y);
        MaxXY = new Point2D(maxXYZ.X, maxXYZ.Y);

        HeightFromGroundAtBase = minXYZ.Height;
        Width = maxXYZ.X - minXYZ.X + 1;
        Depth = maxXYZ.Y - minXYZ.Y + 1;
        Height = (maxXYZ.Height - minXYZ.Height) + 1;
    }
}

record Point2D(int X, int Y);

record Point(int X, int Y, int Height);

class SandTower
{
    private readonly Block?[,,] _impassable;
    private readonly Block[] _blocks;
    private readonly Block _ground;

    public SandTower(Block[] blocks)
    {
        _blocks = blocks;
        int maxX = blocks.Max(b => b.MaxXY.X);
        int maxY = blocks.Max(b => b.MaxXY.Y);
        _impassable = new Block[maxX + 1, maxY + 1, blocks.Max(b => b.HeightFromGroundAtBase + b.Height) + 1];
        
        foreach (var block in blocks)
        {
            for (int x = block.MinXY.X; x <= block.MaxXY.X; x++)
            {
                for (int y = block.MinXY.Y; y <= block.MaxXY.Y; y++)
                {
                    _impassable[x, y, block.HeightFromGroundAtBase] = block;
                    if (block.Height > 1)
                    {
                        _impassable[x, y, block.HeightFromGroundAtBase + block.Height - 1] = block;
                    }
                }
            }
        }

        _ground = new Block("Ground", new Point(0, 0, 0), new Point(maxX, maxY, 0));
        for (int x = 0; x <= maxX; x++)
        {
            for (int y = 0; y <= maxY; y++)
            {
                _impassable[x, y, 0] = _ground;
            }
        }
    }

    public void Report()
    {
        foreach (var block in _blocks)
        {
            Console.WriteLine($"{block.Label} at height {block.HeightFromGroundAtBase}. " +
                              $"Supported By {string.Join(',', GetBlocksSupporting(block).Select(b => b.Label))}. " +
                              $"Supporting {string.Join(',', GetBlocksSupportedBy(block).Select(b => b.Label))}");
        }
    }

    public void SettlePile()
    {
        var unsupported = new Queue<Block>(_blocks.Where(b => !IsSupported(b)));
        while (unsupported.Count > 0)
        {
            var block = unsupported.Dequeue();
            var supportedBlocks = GetBlocksSupportedBy(block).ToArray();

            while (!IsSupported(block))
            {
                var impassablePoints = GetImpassablePointsForBlock(block).ToArray();
                block.HeightFromGroundAtBase--;
                
                foreach (var point in impassablePoints)
                {
                    _impassable[point.X, point.Y, point.Height] = null;
                    _impassable[point.X, point.Y, point.Height - 1] = block;
                }
            }
            
            foreach(var newUnsupported in supportedBlocks.Where(b => !IsSupported(b)))
            {
                unsupported.Enqueue(newUnsupported);
            }
        }
    }

    public int CountRemovable()
    {
        return _blocks.Count(b =>
        {
            var blocksSupported = GetBlocksSupportedBy(b).ToList();
            return blocksSupported.Count == 0 ||
                   blocksSupported.All(x => GetBlocksSupporting(x).Count() > 1);
        });
    }

    private IEnumerable<Block> GetBlocksSupporting(Block block)
    {
        foreach (var supportingBlock in GetPointsAboveOrBelowBlock(block, true)
                     .Select(p => _impassable[p.X, p.Y, p.Height])
                     .Where(b => b != null)
                     .Distinct())
        {
            yield return supportingBlock;
        }
    }
    
    private IEnumerable<Block> GetBlocksSupportedBy(Block block)
    {
        foreach (var supportedBlock in GetPointsAboveOrBelowBlock(block, false)
                     .Select(p => _impassable[p.X, p.Y, p.Height])
                     .Where(b => b != null)
                     .Distinct())
        {
            yield return supportedBlock;
        }
    }
    
    private bool IsSupported(Block block)
    {
        return GetPointsAboveOrBelowBlock(block, true).Any(p => _impassable[p.X, p.Y, p.Height] != null);
    }

    private IEnumerable<Point> GetPointsAboveOrBelowBlock(Block block, bool below)
    {
        for (int x = block.MinXY.X; x <= block.MaxXY.X; x++)
        {
            for (int y = block.MinXY.Y; y <= block.MaxXY.Y; y++)
            {
                yield return new Point(x, y, below ? block.HeightFromGroundAtBase - 1 : block.HeightFromGroundAtBase + block.Height);
            }
        }
    }
    
    private IEnumerable<Point> GetImpassablePointsForBlock(Block block)
    {
        for (int x = block.MinXY.X; x <= block.MaxXY.X; x++)
        {
            for (int y = block.MinXY.Y; y <= block.MaxXY.Y; y++)
            {
                yield return new Point(x, y, block.HeightFromGroundAtBase);
                if (block.Height > 1)
                {
                    yield return new Point(x, y, block.HeightFromGroundAtBase + block.Height - 1);
                }
            }
        }
    }
}