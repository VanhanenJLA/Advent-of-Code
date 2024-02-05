using System.Text.RegularExpressions;
using Common;

namespace Advent_of_Code._2023._3;

public class Tests : TestBase
{
    private const string Example = "467..114..\n...*......\n..35..633.\n......#...\n617*......\n.....+.58.\n..592.....\n......755.\n...$.*....\n.664.598..";

    [Theory]
    [InlineData(Example, "", Level.PartOne)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }
}

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        var schematic = GetSchematic(input);
    
        // var y = 1;
        // var x = 3;
        
        for (int y = 0; y < schematic.GetLength(0); y++)
        {
            for (int x = 0; x < schematic.GetLength(1); x++)
            {
                var current = schematic[y, x];
                // if (!IsSymbol(current)) continue;
                schematic.PrintMatrix((y,x));
            }
        }
        
        // var numbers = FindSurroundingNumberGroups(y, x, schematic).ToList();
        return "";
    }

    private static IEnumerable<string> FindSurroundingNumberGroups(int y, int x, char[,] schematic)
    {
        int[] dx = { -1, 0, 1, 0, -1, -1, 1, 1 }; // Offsets for adjacent and diagonal columns
        int[] dy = { 0, -1, 0, 1, -1, 1, -1, 1 }; // Offsets for adjacent and diagonal rows

        for (int i = 0; i < 8; i++)
        {
            int ny = y + dy[i]; // Calculate the new row index
            int nx = x + dx[i]; // Calculate the new column index

            // Check if the new indices are within the bounds of the array
            if (ny >= 0 && ny < schematic.GetLength(0) && nx >= 0 && nx < schematic.GetLength(1))
            {
                // Check if the neighboring tile is a digit
                if (char.IsDigit(schematic[ny, nx]))
                {
                    // Concatenate adjacent digits until a non-digit character is encountered
                    string numberGroup = schematic[ny, nx].ToString();

                    // Continue checking adjacent digits in the same direction
                    while (true)
                    {
                        ny += dy[i];
                        nx += dx[i];

                        // Check if the new indices are within the bounds of the array
                        if (ny >= 0 && ny < schematic.GetLength(0) && nx >= 0 && nx < schematic.GetLength(1))
                        {
                            if (char.IsDigit(schematic[ny, nx]))
                            {
                                numberGroup += schematic[ny, nx];
                            }
                            else
                            {
                                break; // Stop if a non-digit character is encountered
                            }
                        }
                        else
                        {
                            break; // Stop if the indices go out of bounds
                        }
                    }

                    // Yield the surrounding number group
                    yield return numberGroup;
                }
            }
        }
    }

    bool IsSymbol(char c)
    {
        // const string pattern = @"[^\d.]";
        const string pattern = @"[*/$%+&/@=#-]";
        return Regex.IsMatch(c.ToString(), pattern);
    }

    private static char[,] GetSchematic(string input)
    {
        var rows = input.Split('\n');
        var schematic = new char[rows.Length, rows[0].Length];
        
        for (var i = 0; i < rows.Length; i++)
        {
            for (var j = 0; j < rows[i].Length; j++)
            {
                schematic[i, j] = rows[i][j];
            }
        }
        return schematic;
    }
}