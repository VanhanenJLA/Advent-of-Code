using Common;
using HtmlAgilityPack;
using Engine.Integrations;

namespace Engine;

public static class PuzzleEngine
{
    private static AdventOfCodeAPI api => new(File.ReadAllText(PathsProvider.GetCookieFilePath()));

    public static HtmlNodeCollection ParseInstructions(string content)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        var nodes = doc.DocumentNode.SelectNodes("//article[@class='day-desc']");
        if (nodes is null || !nodes.Any())
            throw new Exception("Article node not found.");
        return nodes;
    }

    public static void SaveInstructions((int year, int day) options, HtmlNodeCollection articles)
    {
        var template = new HtmlDocument();
        template.Load(PathsProvider.GetInstructionsTemplateFilePath());

        var body =template
            .DocumentNode
            .SelectSingleNode("//body");

        foreach (var a in articles)
        {
            body.AppendChild(a);
        }

        File.WriteAllText(PathsProvider.GetInstructionsFilePath(options), template.DocumentNode.OuterHtml);
    }

    public static async Task<bool> SaveInput(string input, (int year, int day) options)
    {
        await File.WriteAllTextAsync(PathsProvider.GetInputFilePath(options), input.ReplaceLineEndings());
        return true;
    }

    private static async Task<string> GetInput((int year, int day) options)
    {
        var path = PathsProvider.GetInputFilePath(options);
        if (File.Exists(path))
        {
            return await File.ReadAllTextAsync(path);
        }

        var input = await api.GetInput(options);
        await SaveInput(input, options);
        return input;
    }

    private static async Task<string> GetInstructions((int year, int day) options)
    {
        var path = PathsProvider.GetInstructionsFilePath(options);
        if (File.Exists(path))
        {
            return await File.ReadAllTextAsync(path);
        }

        var content = await api.GetInstructions(options);
        var instructions = ParseInstructions(content);
        SaveInstructions(options, instructions);
        return await File.ReadAllTextAsync(path);
    }

    public static async Task<bool> SubmitAnswer(string answer, (int year, int day) options, Level level = Level.PartOne)
    {
        var (year, day) = options;
        var response = await api.SubmitAnswer(answer, (year, day), level);

        if (response.Contains(Constants.CorrectAnswerResponse))
            return true;

        if (response.Contains(Constants.IncorrectAnswerResponse))
            return false;

        if (response.Contains(Constants.AlreadySolvedResponse))
            return true;

        throw new Exception($"Unmapped response: {response}");
    }

    public static async Task<bool> CreateSolution((int year, int day) options)
    {
        var (year, day) = options;
        var content = (await File
                .ReadAllTextAsync(PathsProvider.GetSolutionTemplatePath()))
            .Replace(Constants.YearToken, year.ToString())
            .Replace(Constants.DayToken, day.ToString());
        var dir = Path.Combine(PathsProvider.GetSolutionsProjectRootDirectory(), year.ToString(), day.ToString());

        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, Constants.SolutionFileName);
        if (File.Exists(file))
            throw new Exception($"Solution file already exists: {file}");
        await File.WriteAllTextAsync(file, content);
        return true;
    }

    public static async Task<bool> Start((int year, int day) options)
    {
        var (year, day) = options;
        var success = await CreateSolution((year, day));
        if (!success) throw new Exception("Solution creation failed.");
        await GetInput((year, day));
        await GetInstructions((year, day));
        return true;
    }
}

public class Tests
{
    private static AdventOfCodeAPI api => new(File.ReadAllText(PathsProvider.GetCookieFilePath()));
    
    private const int Year = 2023;
    private const int Day = 5;

    [Fact]
    public async Task Should_fetch_and_save_puzzle_input()
    {
        var input = await api.GetInput((Year, Day));
        Assert.NotEmpty(input);
        var path = PathsProvider.GetInputFilePath((Year, Day));
        var exists = File.Exists(path);
        Assert.True(exists);
    }

    [Fact]
    public async Task Should_fetch_and_save_puzzle_instructions()
    {
        var content = await api.GetInstructions((Year, Day));
        Assert.NotEmpty(content);
        var path = PathsProvider.GetInstructionsFilePath((Year, Day));
        var exists = File.Exists(path);
        Assert.True(exists);
    }

    [Theory]
    [InlineData("23750", 2023, 4, Level.PartOne)]
    public async Task Should_submit_correctly(string answer, int year, int day, Level level)
    {
        var correct = await PuzzleEngine.SubmitAnswer(answer, (year, day), level);
        Assert.True(correct);
    }

    [Theory]
    [InlineData(2025, 1)]
    public async Task Should_scaffold_new_solution(int year, int day)
    {
        // Check if exists
        var success = await PuzzleEngine.Start((year, day));
        Assert.True(success);
    }
}