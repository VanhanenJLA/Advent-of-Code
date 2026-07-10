using Common;

namespace Advent_of_Code._2023._8;

public class Tests : TestBase
{
    private const string Example = "LLR\n\nAAA = (BBB, BBB)\nBBB = (AAA, ZZZ)\nZZZ = (ZZZ, ZZZ)";

    [Theory]
    [InlineData(null, "21883", Level.PartOne)]
    [InlineData(Example, "6", Level.PartOne)]
    [InlineData(null, "TBD", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }
}

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
       var parts = input.Split("\n\n",  StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
       var directions = parts[0].Select(ParseDirection).ToArray();
       var nodeRows = parts[1];
       var links = new Dictionary<string, (string L, string R)>();

       foreach (var node in nodeRows.Split('\n',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
       {
           var nodeParts = node.Split(" = ", StringSplitOptions.TrimEntries);
           var children = nodeParts[1]
               .Trim('(', ')')
               .Split(", ", StringSplitOptions.TrimEntries);

           links[nodeParts[0]] = (children[0], children[1]);
       }
       
       var current = "AAA";
       const string last = "ZZZ";
       var steps = 0;
       do
       {
           var direction = directions[steps % directions.Length];
           var next = direction switch
           {
               Direction.Left => links[current].L,
               Direction.Right => links[current].R,
               _ => string.Empty
           };
           current = next;
           steps++;
       } while (current != last);
       return steps.ToString();
    }
    
    public Direction ParseDirection(char c)                               
    {                                                                     
        return c switch                                                   
        {                                                                 
            'L' => Direction.Left,                                        
            'R' => Direction.Right,                                       
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
        };                                                                
    }                                                                     
}

public enum Direction
{
    Left,
    Right
}







