using System.Collections;
using Common;

namespace Advent_of_Code._2023._2;

public class Tests : TestBase
{
    private const string ExampleInput =
        @"Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green";

    [Theory]
    [InlineData(ExampleInput, "8", Level.PartOne)]
    [InlineData(null, "3059", Level.PartOne)]
    [InlineData(null, "TBD", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }

    [Theory]
    [InlineData("Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green", 48)]
    [InlineData("Game 2: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red", 12)]
    private void Should_calculate_power(string gameString, int expected)
    {
        var solution = new Solution();
        var game = solution.ParseGame(gameString);
        var minCubes = solution.FindMinCubes(game);
        var power = minCubes.Red * minCubes.Green * minCubes.Blue;
        Assert.Equal(expected, power);
    }
    
    [Theory]
    [InlineData("Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green", true)]
    [InlineData("Game 2: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red", false)]
    private void Should_determine_game_possibility(string gameString, bool isPossible)
    {
        var solution = new Solution();
        var game = solution.ParseGame(gameString);
        Assert.Equal(solution.IsPossible(game), isPossible);
    }

    [Theory]
    [InlineData("Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green")]
    private void Should_parse_game(string game)
    {
        var solution = new Solution();
        var set1 = new CubeSet(4, 0, 3);
        var set2 = new CubeSet(1, 2, 6);
        var set3 = new CubeSet(0, 2, 0);
        var expected = new Game(1, new[] { set1, set2, set3 });
        var actual = solution.ParseGame(game);
        
        Assert.True(expected.Id == actual.Id);
        Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(expected.Sets, actual.Sets));
    }

    [Theory]
    [InlineData("1 red, 2 green, 6 blue")]
    private void Should_parse_set(string set)
    {
        var expected = new CubeSet(1, 2, 6);
        Assert.Equal(expected, Solution.ParseSet(set));
    }
}

public record Game(int Id, CubeSet[] Sets);

public record CubeSet(int Red, int Green, int Blue);

public class Solution : ISolution
{
    private static CubeSet Max => new(12, 13, 14);
    
    public string Solve(string input, Level level)
    {
        var games = input
            .Split("\n")
            .Select(ParseGame);

        if (level == Level.PartOne)
            return games
                .Where(IsPossible)
                .Select(g => g.Id)
                .Sum()
                .ToString();

        return games
            .Select(FindMinCubes)
            .Select(s => s.Red * s.Green * s.Blue)
            .Sum()
            .ToString();
    }

    public CubeSet FindMinCubes(Game game) // Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
    {
        var min = (0, 0, 0);
        foreach (var set in game.Sets)
        {
            min.Item1 = Math.Max(min.Item1, set.Red);
            min.Item2 = Math.Max(min.Item2, set.Green);
            min.Item3 = Math.Max(min.Item3, set.Blue);
        }
        return new CubeSet(min.Item1, min.Item2, min.Item3);
    }
    
    public bool IsPossible(Game game)
    {
        // "3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green"
        return game
            .Sets
            .All(IsPossible);
        
        bool IsPossible(CubeSet set) =>
            set.Red <= Max.Red
            && set.Green <= Max.Green
            && set.Blue <= Max.Blue;
    }
    
    public Game ParseGame(string game) // "Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green"
    {
        var parts = game.Split(": ");
        var id = int.Parse(parts[0].Replace("Game ", ""));
        var sets = parts[1]
            .Split("; ")
            .Select(ParseSet)
            .ToArray();
        return new Game(id, sets);
    }

    public static CubeSet ParseSet(string set) // "1 red, 2 green, 6 blue"
    {
        var countAndColorPairs = set
            .Split(", ")
            .Select(countAndColor => countAndColor.Split(" "))
            .ToDictionary(countAndColor => countAndColor[1], colorCount => int.Parse(colorCount[0]));

        var red = countAndColorPairs.GetValueOrDefault("red", 0);
        var green = countAndColorPairs.GetValueOrDefault("green", 0);
        var blue = countAndColorPairs.GetValueOrDefault("blue", 0);

        return new CubeSet(red, green, blue);
    }
}