using Advent_of_Code;
using Advent_of_Code._2022._1;
using Common;
using HtmlAgilityPack;
using static Common.Constants;

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
    
}