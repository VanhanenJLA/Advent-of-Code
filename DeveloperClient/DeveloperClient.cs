using System.Runtime.CompilerServices;
using Common;
using HtmlAgilityPack;
using API;

namespace DeveloperClient;

public static class DeveloperClient
{
    private static AdventOfCodeAPI api = new(File.ReadAllText(GetCookieFilePath()));
    
    public static void ParseAndSaveInstructions(string content, (int year, int day) options)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);
        var articleNode = doc.DocumentNode.SelectSingleNode("//article[@class='day-desc']");

        var template = new HtmlDocument();
        template.Load(GetInstructionsTemplateFilePath());

        template
            .DocumentNode
            .SelectSingleNode("//body")
            .AppendChild(articleNode);

        File.WriteAllText(GetInstructionsFilePath(options), template.DocumentNode.OuterHtml);
    }
    
    public static async Task<bool> SaveInput(string input, (int year, int day) options)
    {
        await File.WriteAllTextAsync(GetInputFilePath(options), input.ReplaceLineEndings());
        return true;
    }

    private static async Task<string> GetInput((int year, int day) options)
    {
        var (year, day) = options;
        return await api.GetInput((year, day));
    }

    public static async Task<bool> SubmitAnswer(string answer, (int year, int day) options, Level level = Level.PartOne)
    {
        var (year, day) = options;
        var response = await api.SubmitAnswer(answer, (year, day), level);

        if (response.Contains(CorrectAnswerResponse))
            return true;
        
        if (response.Contains(IncorrectAnswerResponse))
            return false;
        
        // if (response.Contains(AlreadySolvedResponse))

        throw new Exception($"Unmapped response: {response}");
    }

    public static async Task<bool> CreateSolution((int year, int day) options)
    {
        var (year, day) = options;
        var content = (await File
                .ReadAllTextAsync(GetSolutionTemplatePath()))
            .Replace(YearToken, year.ToString())
            .Replace(DayToken, day.ToString());
        var dir = Path.Combine(GetSolutionsProjectRootDirectory(), year.ToString(), day.ToString());
        
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, SolutionFileName);
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
        var input = await GetInput((year, day));
        success = await SaveInput(input, (year, day));
        if (!success) throw new Exception("Input.txt creation failed.");
        return true;
    }
    
}

public class Tests
{
    private static AdventOfCodeAPI api = new(File.ReadAllText(GetCookieFilePath()));
    private const int year = 2023;
    const int day = 5;
    
    [Fact]
    public async Task Should_fetch_and_save_puzzle_input()
    {
        var input = await api.GetInput((year, day));
        Assert.NotEmpty(input);
        var success = await DeveloperClient.SaveInput(input, (year, day));
        Assert.True(success);
        var path = GetInputFilePath((year, day));
        var exists = File.Exists(path);
        Assert.True(exists);
    }
    
    [Fact]
    public async Task Should_fetch_and_save_puzzle_instructions()
    {
        var content = await api.GetInstructions((year, day));
        DeveloperClient.ParseAndSaveInstructions(content, (year, day));
        var path = GetInstructionsFilePath((year, day));
        var exists = File.Exists(path);
        Assert.True(exists);
    }
    
    [Theory]
    [InlineData("23750", 2023, 04)]
    public async Task Should_submit_correctly(string answer, int year, int day)
    {
        var correct = await DeveloperClient.SubmitAnswer(answer, (year, day));
        Assert.True(correct);
    }
    
    [Theory]
    [InlineData(2023, 7)]
    public async Task Should_scaffold_new_solution(int year, int day)
    {
        var success = await DeveloperClient.Start((year, day));
        Assert.True(success);
    }
    
}