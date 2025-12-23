using Common;

namespace Advent_of_Code._2025._1;

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
        throw new NotImplementedException();
    }
    
}