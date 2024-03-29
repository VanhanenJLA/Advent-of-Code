using System.Text.RegularExpressions;
using Common;

namespace Advent_of_Code._2023._3;

public class Tests : TestBase
{
    private const string Example = "467..114..\n...*......\n..35..633.\n......#...\n617*......\n.....+.58.\n..592.....\n......755.\n...$.*....\n.664.598..";

    [Theory]
    [InlineData(Example, "4361", Level.PartOne)]
    [InlineData(null, "TBD", Level.PartOne)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }
    
    [Theory]
    [InlineData(Example)]
    public void Should_iterate_and_print_matrix(string schematic)
    {
        var matrix = Solution.GetSchematic(schematic);
        for (var y = 0; y < matrix.GetLength(0); y++)
        {
            for (var x = 0; x < matrix.GetLength(1); x++)
            {
                matrix.PrintMatrix((y,x));
                Thread.Sleep(250);
            }
        }
    }
}

record PartNumber((int y, int x) Start, string Number)
{
    public string Number { get; set; } = Number;
}

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        var schematic = GetSchematic(input);

        var numbers = GetNumbers(schematic);

        var partNumbers = numbers.Where(IsPartNumber);
        
        return partNumbers
            .Select(pn => int.Parse(pn.Number))
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
    

    private IEnumerable<PartNumber> GetNumbers(char[,] schematic)
    {
        
        var pns = new List<PartNumber>();
        PartNumber pn = null;
        
        for (var y = 0; y < schematic.GetLength(0); y++)
        {
            for (var x = 0; x < schematic.GetLength(1); x++)
            {
                var current = schematic[y, x];
                
                if (!char.IsNumber(current) && pn == null) 
                    continue;
                
                if (!char.IsNumber(current) && pn != null)
                {
                    pns.Add(pn);
                    pn = null;
                    continue;
                }

                if (char.IsNumber(current) && pn == null)
                {
                    pn = new PartNumber((y, x), current.ToString());
                    continue;
                }

                if (char.IsNumber(current) && pn != null)
                {
                    pn.Number += current.ToString();
                }
            }
        }

        if (pn != null) // If schematic's last tile contained a number
        {
            pns.Add(pn); // Capture the last part number.
            pn = null;
        }

        return pns;
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