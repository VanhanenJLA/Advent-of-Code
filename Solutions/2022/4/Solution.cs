using System.Text.RegularExpressions;
using Common;

namespace Advent_of_Code._2022._4;

public class Tests : TestBase
{
    [Theory]
    [InlineData("2-4,6-8\n2-3,4-5\n5-7,7-9\n2-8,3-7\n6-6,4-6\n2-6,4-8", "2", Level.PartOne)]
    [InlineData(null, "453", Level.PartOne)]
    [InlineData(null, "TBD", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }
    
}

public class Solution : ISolution
{
    private record Range(int Start, int End);
    
    private delegate bool OverlapPredicate((Range, Range) pair);
    
    public string Solve(string input, Level level)
    {
        var rows = input.Split("\n");
        var assignments = rows.Select(Parse);

        var overlappingPredicate = Get(level);
        
        return assignments
            .Count(p => overlappingPredicate(p))
            .ToString();
    }
    
    static (Range, Range) Parse(string row) // "2-4,6-8"
    {
        var parts = row.Split(",");
        var first = ParseAssignment(parts[0]);
        var second = ParseAssignment(parts[1]);
        return (first, second);
    }

    static Range ParseAssignment(string assignment) // "2-4"
    {
        var parts = assignment.Split("-");
        var start = int.Parse(parts[0]);
        var end = int.Parse(parts[1]);
        return new Range(start, end);
    }

    static bool IsSubset((Range a, Range b) pair)
    {
        var (a, b) = pair;
        return (a.Start <= b.Start && a.End >= b.End) ||
               (b.Start <= a.Start && b.End >= a.End);
    }

    static bool IsOverlapping((Range a, Range b) pair)
    {
        var (a, b) = pair;
        return a.Start <= b.End && a.End >= b.Start;
    }
    
    static OverlapPredicate Get(Level level) =>
        level switch
        {
            Level.PartOne => IsSubset,
            Level.PartTwo => IsOverlapping,
            _ => throw new ArgumentException("Unmapped level: " + level)
        };
    
}