using Common;
using Microsoft.Extensions.Logging.Abstractions;
using static Common.PathsProvider;

namespace Engine.Tests;

public class PuzzleEngineTests
{
    private static PuzzleEngine PuzzleEngine => new(NullLogger<PuzzleEngine>.Instance);
    
    private const int Year = 2020;
    private const int Day = 20;
    private const int SandboxYear = 2099;

    [Fact]
    public async Task Should_fetch_and_save_puzzle_input()
    {
        var input = await PuzzleEngine.GetInput((Year, Day));
        Assert.NotEmpty(input);
        var path = GetInputFilePath((Year, Day));
        var exists = File.Exists(path);
        Assert.True(exists);
    }

    [Fact]
    public async Task Should_fetch_and_save_puzzle_instructions()
    {
        var content = await PuzzleEngine.GetInstructions((Year, Day));
        Assert.NotEmpty(content);
        var path = GetInstructionsFilePath((Year, Day));
        var exists = File.Exists(path);
        Assert.True(exists);
    }

    [Theory]
    [InlineData("23750", 2023, 4, Level.PartOne)]
    public async Task Submitting_correct_answer_should_succeed(string answer, int year, int day, Level level)
    {
        var correct = await PuzzleEngine.SubmitAnswer(answer, (year, day), level);
        Assert.True(correct);
    }

    [Theory]
    [InlineData(2020, 20)]
    public async Task Should_scaffold_new_solution(int year, int day)
    {
        await PuzzleEngine.Unstart((year, day));

        var success = await PuzzleEngine.Start((year, day));
        Assert.True(success);
    }

    [Fact]
    public async Task Should_remove_entire_year_when_day_is_omitted()
    {
        var yearDirectory = Path.Combine(GetSolutionsProjectRootDirectory(), SandboxYear.ToString());
        var dayDirectory = Path.Combine(yearDirectory, "1");

        if (Directory.Exists(yearDirectory))
            Directory.Delete(yearDirectory, true);

        Directory.CreateDirectory(dayDirectory);
        await File.WriteAllTextAsync(Path.Combine(dayDirectory, "input.txt"), "test");

        var success = await PuzzleEngine.Unstart((SandboxYear, null));

        Assert.True(success);
        Assert.False(Directory.Exists(yearDirectory));
    }
}
