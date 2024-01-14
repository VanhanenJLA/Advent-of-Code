using Common;

namespace Advent_of_Code._2022._1;

public class Tests : TestBase
{
    // Part One
    [Theory]
    [InlineData("1000\n2000\n3000\n\n4000\n\n5000\n6000\n\n7000\n8000\n9000\n\n10000", "24000", Level.PartOne)]
    // [InlineData(Input._2022_1, "", Level.PartOne)]
    public override void Should_give_correct_answer(string input, string expected, Level level)
    {
        var answer = Solution.Solve(input, level);
        Assert.Equal(expected, answer);
    }
}

public static class Solution
{
    public static string Solve(string input, Level level)
    {
        var groups = input.Split("\n\n");
        var calories = groups
            .Select(group =>
                {
                    var calories = group.Split("\n");
                    var numbers = calories.Select(int.Parse);
                    return numbers.Sum();
                }
            );
        return calories.Max().ToString();
    }
}