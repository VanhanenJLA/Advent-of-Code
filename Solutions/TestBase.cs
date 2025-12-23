using Common;
using static Common.Constants;

namespace Advent_of_Code;

public abstract class TestBase
{
    public abstract void Should_solve_correct_answer(string? input, string expected, Level level);

    protected void DefaultTest(string? input, string expected, Level level)
    {
        var (year, day) = PathsProvider.GetYearAndDay();
        var solutionName = $"Advent_of_Code._{year}._{day}.Solution";
        var type = Type.GetType(solutionName)
                   ?? throw new Exception($"Solution '{solutionName}' not found");
        var solution = Activator.CreateInstance(type) as ISolution
                       ?? throw new Exception($"Solution Instance Creation failed.");
        
        input = Initialize(input);
        var answer = solution.Solve(input, level);
        Assert.Equal(expected, answer);
    }


    private static string Initialize(string? input)
    {
        var inputFilePath = PathsProvider
            .GetSolutionFilePath()
            .Replace(SolutionFileName, InputTextFileName);
        return input
            ?? File.ReadAllText(inputFilePath)
            ?? throw new ArgumentException($"Puzzle input was not provided inline and locally stored input was not found at path: '{inputFilePath}'");
    }

}