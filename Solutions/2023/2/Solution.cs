using Advent_of_Code._2023._1;
using Common;

namespace Advent_of_Code._2023._2;

public class Tests
{
    private const string exampleInput =
        @"Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green";

    // Part One
    [Theory]
    [InlineData(exampleInput, "8", Level.PartOne)]
    [InlineData(Input._2023_2, "3059", Level.PartOne)]
    private void Should_give_correct_answer(string input, string expected, Level level)
    {
        var answer = Solution.Solve(input, level);
        Assert.Equal(expected, answer);
    }

    [Theory]
    [InlineData("3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green", true)]
    [InlineData("8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red", false)]
    private void Should_determine_game_possibility(string game, bool isPossible)
    {
        Assert.Equal(Solution.IsPossibleGame(game), isPossible);
    }
}

public static class Solution
{
    private static readonly Dictionary<string, int> MaxColors = new()
    {
        { "red", 12 },
        { "green", 13 },
        { "blue", 14 }
    };

    public static bool IsPossibleGame(string game)
    {
        // "3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green"
        var sets = game
            .Split("; ");
        return sets.All(IsPossibleSet);
    }

    public static bool IsPossibleSet(string set)
    {
        // "1 red, 2 green, 6 blue"
        var colorsAndCounts = set
            .Split(", ")
            .Select(colorCountPair => colorCountPair.Split(" "))
            .ToDictionary(colorCount => colorCount[1], colorCount => int.Parse(colorCount[0]));

        return colorsAndCounts.All(colorCount =>
            !MaxColors.TryGetValue(colorCount.Key, out var maxCount) || colorCount.Value <= maxCount);
    }

    public static string Solve(string input, Level level)
    {
        var games = input
            .Split("\n")
            .Select(r => r.Split(": "));

        var possibles = games
            .Where(g => IsPossibleGame(g[1]));

        var sum = possibles
            .Select(p => int.Parse(p[0].Replace("Game ", "")))
            .Sum();
        return sum.ToString();
    }
}