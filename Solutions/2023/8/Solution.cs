using Common;

namespace Advent_of_Code._2023._8;

public class Tests : TestBase
{
    private const string Example = """
        LLR

        AAA = (BBB, BBB)
        BBB = (AAA, ZZZ)
        ZZZ = (ZZZ, ZZZ)
        """;

    private const string PartTwoExample =
        """
        LR

        11A = (11B, XXX)
        11B = (XXX, 11Z)
        11Z = (11B, XXX)
        22A = (22B, XXX)
        22B = (22C, 22C)
        22C = (22Z, 22Z)
        22Z = (22B, 22B)
        XXX = (XXX, XXX)
        """;

    [Theory]
    [InlineData(null, "21883", Level.PartOne)]
    [InlineData(Example, "6", Level.PartOne)]
    [InlineData(PartTwoExample, "6", Level.PartTwo)]
    [InlineData(null, "12833235391111", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }
}

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        var map = Parse(input);

        return level switch
        {
            Level.PartOne => CountSteps(map, "AAA", node => node == "ZZZ").ToString(),
            Level.PartTwo => map.Nodes.Keys
                .Where(node => node.EndsWith('A'))
                .Select(start => CountSteps(map, start, node => node.EndsWith('Z')))
                .Aggregate(LeastCommonMultiple)
                .ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }

    private static Map Parse(string input)
    {
        var parts = input.Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var directions = parts[0].Select(ParseDirection).ToArray();
        var nodes = new Dictionary<string, (string Left, string Right)>();

        foreach (var node in parts[1].Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var nodeParts = node.Split(" = ", StringSplitOptions.TrimEntries);
            var children = nodeParts[1]
                .Trim('(', ')')
                .Split(", ", StringSplitOptions.TrimEntries);

            nodes[nodeParts[0]] = (children[0], children[1]);
        }

        return new Map(directions, nodes);
    }

    private static long CountSteps(Map map, string current, Func<string, bool> isDestination)
    {
        var steps = 0L;

        do
        {
            current = NextNode(map, current, steps);
            steps++;
        } while (!isDestination(current));

        return steps;
    }

    private static string NextNode(Map map, string current, long steps)
    {
        var direction = map.Directions[(int)(steps % map.Directions.Length)];
        var node = map.Nodes[current];

        return direction switch
        {
            Direction.Left => node.Left,
            Direction.Right => node.Right,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    private static Direction ParseDirection(char c)
    {
        return c switch
        {
            'L' => Direction.Left,
            'R' => Direction.Right,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
        };
    }

    private static long GreatestCommonDivisor(long a, long b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }

    private static long LeastCommonMultiple(long a, long b) => a / GreatestCommonDivisor(a, b) * b;

    private sealed record Map(Direction[] Directions, Dictionary<string, (string Left, string Right)> Nodes);
}

public enum Direction
{
    Left,
    Right
}





