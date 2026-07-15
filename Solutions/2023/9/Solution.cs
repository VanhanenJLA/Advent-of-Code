using Common;

namespace Advent_of_Code._2023._9;

public class Tests : TestBase
{
    private const string Example = "0 3 6 9 12 15\n1 3 6 10 15 21\n10 13 16 21 30 45";

    [Theory]
    [InlineData(Example, "114", Level.PartOne)]
    [InlineData(Example, "2", Level.PartTwo)]
    [InlineData(null, "1666172641", Level.PartOne)]
    [InlineData(null, "933", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }

    [Theory]
    [InlineData("0 3 6 9 12 15", "3 3 3 3 3")]
    public void Should_count_diffs(string input, string expected)
    {
        var numbers = input
            .Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToArray();

        var diffs = numbers
            .Zip(numbers.Skip(1), (n1, n2) => n2 - n1)
            .ToArray();

        Assert.Equal(expected, string.Join(" ", diffs));
    }
}

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        Func<int[], int> predict = level switch
        {
            Level.PartOne => PredictNext,
            Level.PartTwo => PredictPrevious,
            _ => throw new ArgumentException($"Cannot predict for level: {level}")
        };

        var result = input
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line
                .Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .ToArray())
            .Sum(predict);

        return result.ToString();
    }

    private static int PredictNext(int[] numbers)
    {
        return GetDiffRows(numbers).Sum(row => row.Last());
    }

    private static int PredictPrevious(int[] numbers)
    {
        return GetDiffRows(numbers)
            .AsEnumerable()
            .Reverse()
            .Aggregate(0, (previous, row) => row.First() - previous);
    }

    private static List<int[]> GetDiffRows(int[] numbers)
    {
        var rows = new List<int[]> { numbers };
        while (rows.Last().Any(n => n != 0))
            rows.Add(GetDiffs(rows.Last()));
        return rows;
    }

    private static int[] GetDiffs(int[] numbers)
    {
        return numbers
            .Zip(numbers.Skip(1), (n1, n2) => n2 - n1)
            .ToArray();
    }
}
