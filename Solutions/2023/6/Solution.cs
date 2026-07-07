using Common;

namespace Advent_of_Code._2023._6;

public class Tests : TestBase
{
    private const string Example = "Time:      7  15   30\nDistance:  9  40  200";

    [Theory]
    [InlineData(Example, "288", Level.PartOne)]
    [InlineData(null, "781200", Level.PartOne)]
    [InlineData(Example, "71503", Level.PartTwo)]
    [InlineData(null, "49240091", Level.PartTwo)]
    
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }

    [Theory]
    [InlineData(7, 9, 4)]
    [InlineData(15, 40, 8)]
    [InlineData(30, 200, 9)]
    public void Should_count_wins(long time, long distance, long expected)
    {
        var wins = Solution.CountWins(new Race(time, distance));
        Assert.Equal(expected, wins);
    }
    
}

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        var wins = Parse(input, level)
            .Select(CountWins);

        var ways = wins
            .Aggregate(1L, (acc, cur) => acc * cur);
        return ways.ToString();
    }

    public static long CountWins(Race r)
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

    private static IEnumerable<Race> Parse(string input, Level level)
    {
        var rows = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var times = ParseNumbers(rows.First(), level);
        var distances = ParseNumbers(rows.Last(), level);

        return times.Zip(distances, (time, distance) => new Race(time, distance));
    }

    private static IEnumerable<long> ParseNumbers(string row, Level level)
    {
        var numbers = row
            .Split(':')[1]
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return level switch
        {
            Level.PartOne => numbers.Select(long.Parse),
            Level.PartTwo => [long.Parse(string.Concat(numbers))],
            _ => throw new ArgumentException($"Cannot parse numbers for level: {level}")
        };
    }
    
}

public record Race(long Time, long Distance);
