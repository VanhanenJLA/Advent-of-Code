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
        var (year, day, type) = options;
        if (type == InputFile.Type.Class)
        {
            input = WrapInClassTemplate(input);

            string WrapInClassTemplate(string input)
            {
                return File
                    .ReadAllText(GetInputClassTemplatePath())
                    .Replace(YearToken, year.ToString())
                    .Replace(DayToken, day.ToString())
                    .Replace(InputToken, input.TrimEnd('\n'));
            }
        }
        File.WriteAllText(GetInputFilePath(options), input);
    }
}

public class Tests
{
    const int year = 2023;
    const int day = 2;
    
    [Fact]
    public async void Should_solve_correctly()
    {
        var input = await API.GetInput((year, day));
        var answer = Solution.Solve(input, Level.PartOne);
        // SUBMIT ANSWER
        // VERIFY RESPONSE
    }
    
    [Theory]
    // [InlineData(InputFile.Type.Text)]
    [InlineData(InputFile.Type.Class)]
    public async void Should_fetch_and_save_puzzle_input(InputFile.Type t)
    {
        var input = await API.GetInput((year, day));
        Assert.NotEmpty(input);
        Client.SaveInput(input, (year, day, t));
        var path = GetInputFilePath((year, day, t));
        var exists = File.Exists(path);
        Assert.True(exists);
    }
    
    [Fact]
    public async void Should_fetch_and_save_puzzle_instructions()
    {
        var content = await API.GetInstructions((year, day));
        Client.ParseAndSaveInstructions(content, (year, day));
        var path = GetInstructionsFilePath((year, day));
        var exists = File.Exists(path);
        Assert.True(exists);
    }
    
    [Fact]
    public async void TEST_SUBMIT_ANSWER()
    {
        var response = await API.SubmitAnswer("123", Level.PartOne, (year, day));
        Console.WriteLine(response);
    }
    
}