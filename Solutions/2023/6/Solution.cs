using Common;

namespace Advent_of_Code._2023._6;

public class Tests : TestBase
{
    private const string Example = "Time:      7  15   30\nDistance:  9  40  200";

    [Theory]
    [InlineData(Example, "288", Level.PartOne)]
    // [InlineData(Example, "71503", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }

    [Theory]
    [InlineData(7, 9, 4)]
    [InlineData(15, 40, 8)]
    [InlineData(30, 200, 9)]
    public void Should_count_wins(int time, int distance, int expected)
    {
        var wins = Solution.CountWins(new Race(time, distance));
        Assert.Equal(expected, wins);
    }
}

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        var races = Parse(input);
        var wins = races
            .Select(CountWins);

        var ways = wins
            .Aggregate(1, (acc, cur) => acc * cur);
        return ways.ToString();
    }

    public static int CountWins(Race r)
    {
        var wins = 0;
        for (var hold = 0; hold <= r.Time; hold++)
        {
            var distance = hold * (r.Time - hold);
            if (distance > r.Distance)
                wins++;
        }

        return wins;
    }

    private static IEnumerable<Race> Parse(string input)
    {
        var rows = input.Split('\n');
        var times = rows
            .First()
            .Split("Time:", StringSplitOptions.RemoveEmptyEntries)
            .First()
            .Split(' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse);

        var distances = rows
            .Last()
            .Split("Distance:", StringSplitOptions.RemoveEmptyEntries)
            .First()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse);

        var dts = times.Zip(distances);
        return dts.Select(d => new Race(d.First, d.Second));
    }
}

public record Race(int Time, int Distance);