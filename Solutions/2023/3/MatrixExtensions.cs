using System.Text;

namespace Advent_of_Code._2023._3;

public static class MatrixExtensions
{
    public static void PrintMatrix<T>(this T[,] matrix) => matrix.PrintMatrix((-1, -1));

    public static void PrintMatrix<T>(this T[,] matrix, (int y, int x) current)
    {
        var rows = matrix.GetLength(0);
        var columns = matrix.GetLength(1);

        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < columns; x++)
            {
                if ((y, x) == (current.y, current.x))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(matrix[y, x]);
                    Console.ResetColor();
                }
                else
                    Console.Write(matrix[y, x]);
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}
