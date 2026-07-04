using System.Text.RegularExpressions;
using Common;

namespace Advent_of_Code._2023._3;

public class Tests : TestBase
{
    private const string Example = "467..114..\n...*......\n..35..633.\n......#...\n617*......\n.....+.58.\n..592.....\n......755.\n...$.*....\n.664.598..";

    [Theory]
    [InlineData(Example, "4361", Level.PartOne)]
    [InlineData(Example, "467835", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }

    [Theory(Skip = "Debug visualization test; not part of the normal test suite.")]
    // [Theory]
    [InlineData(Example)]
    public void Should_iterate_and_print_matrix(string schematic)
    {
        var matrix = Solution.GetSchematic(schematic);
        for (var y = 0; y < matrix.GetLength(0); y++)
        {
            for (var x = 0; x < matrix.GetLength(1); x++)
            {
                matrix.PrintMatrix((y, x));
                Thread.Sleep(250);
            }
        }
    }
}

public record PartNumber((int y, int x) Start, string Number)
{
    public string Number { get; set; } = Number;
    public (int y, int x) End => (Start.y, Start.x + Number.Length - 1);
}

public record Gear(int y, int x);

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        var schematic = GetSchematic(input);
        var candidateNumbers = GetNumbers(schematic);

        if (level == Level.PartOne)
        {
            var partNumbers = candidateNumbers.Where(IsPartNumber);

            return partNumbers
                .Select(pn => int.Parse(pn.Number))
                .Sum()
                .ToString();
        }

        var gears = new Dictionary<Gear, List<PartNumber>>(); 

        foreach (var pn in candidateNumbers)
        {
            foreach (var g in GetAdjacentGears(pn, schematic))
            {
                if (!gears.ContainsKey(g))
                    gears[g] = [];

                gears[g].Add(pn);
            }
        }

        return gears
            .Values
            .Where(g => g.Count == 2)
            .Select(g => int.Parse(g.First().Number) * int.Parse(g.Last().Number))
            .Sum()
            .ToString();


        bool IsPartNumber(PartNumber pn)
        {
            for (var i = 0; i < pn.Number.Length; i++)
            {
                var (y, x) = pn.Start;
                if (IsConnectedToASymbol((y, x + i)))
                    return true;
            }

            return false;
        }

        bool IsConnectedToASymbol((int y, int x) point)
        {
            var rows = schematic.GetLength(0);
            var columns = schematic.GetLength(1);

            for (var y = point.y - 1; y <= point.y + 1; y++)
            {
                for (var x = point.x - 1; x <= point.x + 1; x++)
                {
                    if (y < 0 || y >= rows || x < 0 || x >= columns)
                        continue; // Out of bounds

                    var current = schematic[y, x];
                    if (IsSymbol(current))
                        return true;
                }
            }

            return false;
        }
    }

    private IEnumerable<Gear> GetAdjacentGears(PartNumber pn, char[,] schematic)
    {
        var rows = schematic.GetLength(0);
        var columns = schematic.GetLength(1);

        var startY = Math.Max(0, pn.Start.y - 1);
        var endY = Math.Min(rows - 1, pn.Start.y + 1);

        var startX = Math.Max(0, pn.Start.x - 1);
        var endX = Math.Min(columns - 1, pn.End.x + 1);

        for (var y = startY; y <= endY; y++)
        {
            for (var x = startX; x <= endX; x++)
            {
                if (schematic[y, x] == '*')
                    yield return new Gear(y, x);
            }
        }
    }

    private IEnumerable<PartNumber> GetNumbers(char[,] schematic)
    {
        var rows = schematic.GetLength(0);
        var columns = schematic.GetLength(1);

        for (var y = 0; y < rows; y++)
        {
            var x = 0;

            while (x < columns)
            {
                if (!char.IsDigit(schematic[y, x]))
                {
                    x++;
                    continue;
                }

                var startX = x;
                var number = "";

                while (x < columns && char.IsDigit(schematic[y, x]))
                {
                    number += schematic[y, x];
                    x++;
                }

                yield return new PartNumber((y, startX), number);
            }
        }
    }

    bool IsSymbol(char c)
    {
        // const string pattern = @"[^\d.]";
        const string pattern = @"[*/$%+&/@=#-]";
        return Regex.IsMatch(c.ToString(), pattern);
    }

    public static char[,] GetSchematic(string input)
    {
        var rows = input.Split('\n');
        var schematic = new char[rows.Length, rows[0].Length];

        for (var y = 0; y < rows.Length; y++)
        {
            for (var x = 0; x < rows[y].Length; x++)
            {
                schematic[y, x] = rows[y][x];
            }
        }

        return schematic;
    }
}