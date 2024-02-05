using Common;

namespace Advent_of_Code;

public abstract class TestBase
{
    public abstract void Should_solve_correct_answer(string? input, string expected, Level level);

    protected void DefaultTest(string? input, string expected, Level level)
    {
        var (year, day) = Utilities.GetYearAndDay();
        var solutionName = $"Advent_of_Code._{year}._{day}.Solution";
        var type = Type.GetType(solutionName)
                   ?? throw new Exception($"Solution '{solutionName}' not found");
        var solution = Activator.CreateInstance(type) as ISolution;
        
        input = HandleInputInitialization(input);
        var answer = solution.Solve(input, level);
        Assert.Equal(expected, answer);
    }


    private string HandleInputInitialization(string? input)
    {
        return input
            ?? File.ReadAllText(Utilities.GetSolutionFilePath().Replace("Solution.cs", Constants.InputTextFileName))
            ?? throw new ArgumentException("No inline input data provided and input.txt was empty or not found.");
    }

}