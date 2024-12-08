using Common;

namespace Advent_of_Code._2022._1;

public class Tests : TestBase
{
    [Theory]
    [InlineData("1000\n2000\n3000\n\n4000\n\n5000\n6000\n\n7000\n8000\n9000\n\n10000", "24000", Level.PartOne)]
    [InlineData("1000\n2000\n3000\n\n4000\n\n5000\n6000\n\n7000\n8000\n9000\n\n10000", "45000", Level.PartTwo)]
    [InlineData(null, "69693", Level.PartOne)]
    [InlineData(null, "200945", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }
}

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        var groups = input.Split("\n\n");
        var calories = groups
            .Select(group =>
                {
                    var calories = group.Split(Environment.NewLine);
                    var numbers = calories.Select(int.Parse);
                    return numbers.Sum();
                }
            );

        if (level == Level.PartOne)
            return calories.Max().ToString();

        return calories
            .OrderByDescending(c => c)
            .Take(3)
            .Sum()
            .ToString();
    }
}