using System.Text.RegularExpressions;
using ConsoleApp;
using ConsoleApp.Day01;
using ConsoleApp.Day02;
using Xunit;

namespace ConsoleApp.Day02;

public class Tests
{
    private const string exampleInput =
        @"Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green";

    // Part One
    [Theory]
    [InlineData(exampleInput, "8", Level.PartOne)]
    private void Should_give_correct_answer(string input, string expectedAnswer, Level level)
    {
        var actual_answer = Solution.Solve(input, level);
        Assert.Equal(expectedAnswer, actual_answer);
    }
}

public static class Cube
{
    public enum Color
    {
        Red,
        Green,
        Blue
    }

    public static Color StringToCubeColor(string color) =>
        color switch
        {
            "red" => Color.Red,
            "green" => Color.Green,
            "blue" => Color.Blue,
            _ => throw new ArgumentException("Unmapped CubeColor: " + color)
        };
}


public static class Solution
{
    private static readonly Dictionary<Cube.Color, int> maxCubes = new()
    {
        { Cube.Color.Red, 12 },
        { Cube.Color.Green, 13 },
        { Cube.Color.Blue, 14 }
    };

    public static string Solve(string input, Level level)
    {
        // return "";


        var games = input.Split("\n");
        var possibleGameSum = 0;
        foreach (var game in games)
        {
            var gameCubes = new Dictionary<Cube.Color, int>();

            var parts = game.Split(": ");
            var id = parts[0].Replace("Game:", "");
            var setsRow = parts[1];
            var sets = setsRow.Split("; ");
            foreach (var set in sets)
            {
                var colorsAndCounts = set.Split(", ");
                var colorAndCount = colorsAndCounts.Split(" ");
                var count = colorAndCount[0];
                var color = colorAndCount[1];
                var cubeColor = Cube.StringToCubeColor(color);
                gameCubes.Add(cubeColor, gameCubes[cubeColor] + int.Parse(count));

                if (gameCubes[cubeColor] > maxCubes[cubeColor])
                {
                    id = "0";
                }

            }
            possibleGameSum += int.Parse(id);
            // input.Split("\n").First()
            //     .Split(": ")
            //     .Split("; ")
            //     .Split(", ")
            //     .Split(" ")
        }
        return possibleGameSum.ToString();
    }
}