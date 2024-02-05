using Common;
using HtmlAgilityPack;

namespace Client;

public static class Client
{
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
    
    public static void SaveInput(string input, (int year, int day, InputFile.Type type) options)
    {
        File.WriteAllText(GetInputFilePath(options), input.TrimEnd('\n'));
    }

    public static async Task<bool> SubmitAnswer(string answer, (int year, int day) options, Level level = Level.PartOne)
    {
        var (year, day) = options;
        var response = await API.SubmitAnswer(answer, (year, day), level);

        if (response.Contains(CorrectAnswerResponse))
            return true;
        
        if (response.Contains(IncorrectAnswerResponse))
            return false;

        throw new Exception("Unmapped response: " + response);
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
            throw new Exception("Solution file already exists: " + file);
        await File.WriteAllTextAsync(file, content);
        return true;
    }
    
}

public class Tests
{
    private const int year = 2023;
    const int day = 4;
    
    [Theory]
    [InlineData(InputFile.Type.Text)]
    public async Task Should_fetch_and_save_puzzle_input(InputFile.Type t)
    {
        var input = await API.GetInput((year, day));
        Assert.NotEmpty(input);
        Client.SaveInput(input, (year, day, t));
        var path = GetInputFilePath((year, day, t));
        var exists = File.Exists(path);
        Assert.True(exists);
    }
    
    [Fact]
    public async Task Should_fetch_and_save_puzzle_instructions()
    {
        var content = await API.GetInstructions((year, day));
        Client.ParseAndSaveInstructions(content, (year, day));
        var path = GetInstructionsFilePath((year, day));
        var exists = File.Exists(path);
        Assert.True(exists);
    }
    
    [Theory]
    [InlineData("23750")]
    public async Task Should_submit_correctly(string answer)
    {
        var correct = await Client.SubmitAnswer(answer, (year, day));
        Assert.True(correct);
    }
    
    [Theory]
    [InlineData(2023, 5)]
    public async Task Should_scaffold_new_solution(int year, int day)
    {
        var success = await Client.CreateSolution((year, day));
        Assert.True(success);
    }
    
}