using Common;
using static Advent_of_Code.Utilities;

namespace Advent_of_Code._2022._01;

public class Tests
{
    const int year = 2022;
    const int day = 1;
    
    [Fact]
    public async void Should_solve_correctly()
    {
        var s = new Solution();
        var input = await API.GetInput((year, day));
        var answer = s.SolvePartOne(input);
        // TODO:
        // SUBMIT ANSWER
        // VERIFY RESPONSE
    }
    
    [Fact]
    public async void Should_fetch_and_save_puzzle_input()
    {
        var input = await API.GetInput((year, day));
        Assert.NotEmpty(input);
        SaveInput(input, (year, day));
        var path = GetInputFilePath((year, day));
        var exists = File.Exists(path);
        Assert.True(exists);
    }
    
    [Fact]
    public async void Should_fetch_and_save_puzzle_instructions()
    {
        
        var content = await API.GetInstructions((year, day));
        ParseAndSaveInstructions(content, (year, day));
        var path = GetInputFilePath((year, day));
        var exists = File.Exists(path);
        Assert.True(exists);
        
    }
    
    [Fact]
    public async void TEST_SUBMIT_ANSWER()
    {
        var response = await API.SubmitAnswer("123", Level.PuzzlePartOne, (year, day));
        Console.WriteLine(response);
    }
    
}