using Common;

namespace Advent_of_Code._2023._6;

public class Tests : TestBase
{

    [Theory]
    [InlineData(null, "TBD", Level.PartOne)]
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
        var dts = Parse(input);
        
        return "";
    }

    private static IEnumerable<(string, string)> Parse(string input)
    {
        var rows = input.Split('\n');
        var times = rows
            .First()
            .Split("Time:", StringSplitOptions.RemoveEmptyEntries)
            .First()
            .Split(' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        var distances = rows
            .Last()
            .Split("Distance:", StringSplitOptions.RemoveEmptyEntries)
            .First()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

           var dts = times.Zip(distances);
           return dts;
    }
}