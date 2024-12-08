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

    [Fact]
    public void Should_parse_and_sort_category()
    {
        var category = ExampleInput.Example
            .Split(Solution.DoublyNewLine, StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .First();

        var cs = Solution
            .ParseCategory(category)
            .OrderBy(c => c.source.Item2);

        var expected = new Solution.Mapping(("seed", 50), ("soil", 52), 48);
        Assert.True(cs.Last().source.Item2 > cs.First().source.Item2);
        Assert.Equal(expected, cs.First());


        // seed-to-soil map:
        // 50 98 2

        // SEED | SOIL
        // 98 => 50
        // 99 => 51
    }
}

public class Solution : ISolution
{
    public static string DoublyNewLine => Environment.NewLine + Environment.NewLine;

    public string Solve(string input, Level level)
    {
        var headers = input.Split(DoublyNewLine, StringSplitOptions.RemoveEmptyEntries);
        var seeds = ParseSeeds(headers.First());

        var categories = headers
            .Skip(1)
            .Select(ParseCategory);

        var seed = seeds.MinBy(s => s);

        foreach (var c in categories)
        {
            Console.WriteLine("Helost");
        }

        throw new NotImplementedException();
    }

    public int Map(int i, Mapping m)
    {
        // if (m.source > )
        return 0;
    }

    public static IEnumerable<int> ParseSeeds(string seeds)
    {
        return seeds
            .Split("seeds: ", StringSplitOptions.RemoveEmptyEntries)
            .First()
            .Split(" ")
            .Select(int.Parse);
    }

    public record Mapping((string, int) source, (string, int) destination, int length);

    public static IEnumerable<Mapping> ParseCategory(string category)
    {
        var header = category
            .Split(" map:", StringSplitOptions.RemoveEmptyEntries)
            .First()
            .Split("-to-", StringSplitOptions.RemoveEmptyEntries);

        var sourceName = header.First();
        var destinationName = header.Last();

        return category
            .Split(" map:", StringSplitOptions.RemoveEmptyEntries)
            .Last()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseMapping)
            .Select(r => new Mapping((sourceName, r.source), (destinationName, r.destination), r.length));
    }

    private record MappingRow(int source, int destination, int length);

    private static MappingRow ParseMapping(string mapping)
    {
        var numbers =
            mapping
                .Split(" ")
                .Select(int.Parse)
                .ToArray();

        var source = numbers[1];
        var dest = numbers[0];
        var length = numbers[2];

        return new(source, dest, length);
    }
}