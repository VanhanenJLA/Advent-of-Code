using Common;

namespace Advent_of_Code._2023._5;

public class Tests : TestBase
{
    [Theory]
    [InlineData(ExampleInput.Example, "35", Level.PartOne)]
    [InlineData(null, "TBD", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }

    [Fact]
    public void Should_parse_seeds()
    {
        const string group = "seeds: 79 14 55 13";
        var expected = new[] { 79, 14, 55, 13 };
        var actual = Solution.ParseSeeds(group);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(ExampleInput.Example, 79, 81)]
    public void Should_map_and_print_seed_to_soil(string? input, int seed, int soil)
    {
        // Seed number 79 corresponds to soil number 81.
        Assert.Fail();
    }
}

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        var categories = input.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        var seeds = ParseSeeds(categories.First());
        throw new NotImplementedException();
    }

    public static IEnumerable<int> ParseSeeds(string seeds)
    {
        return seeds
            .Split("seeds: ", StringSplitOptions.RemoveEmptyEntries)
            .First()
            .Split(" ")
            .Select(int.Parse);
    }

    public static IEnumerable<Mapping> ParseCategory(string category)
    {
        return category
            .Split(" map:", StringSplitOptions.RemoveEmptyEntries)
            .Last()
            .Split("\n")
            .Select(ParseMapping);
    }

    private static Mapping ParseMapping(string mapping)
    {
        var numbers =
            mapping
                .Split(" ")
                .Select(int.Parse)
                .ToArray();
        var (a, b, c) = numbers;
        return new Mapping(numbers[0], numbers[1], numbers[2]);
    }

    public record Mapping(int dest, int source, int length);

    int Map(int i, Mapping m)
    {
        return 0;
    }
}