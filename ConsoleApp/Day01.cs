using System.Text.RegularExpressions;
using Xunit;

namespace ConsoleApp.Day01;

public class Tests
{
    // Part One
    [Theory]
    [InlineData("1abc2\npqr3stu8vwx\na1b2c3d4e5f\ntreb7uchet", "142", Level.PartOne)]
    [InlineData(Input.input_2023_01, "54990", Level.PartOne)]

    // Part Two
    [InlineData("two1nine\neightwothree\nabcone2threexyz\nxtwone3four\n4nineeightseven2\nzoneight234\n7pqrstsixteen",
        "281", Level.PartTwo)]
    [InlineData(Input.input_2023_01, "54473", Level.PartTwo)]
    [InlineData("6zfxp", "66", Level.PartTwo)]
    [InlineData("oneight", "18", Level.PartTwo)]
    [InlineData("twone", "21", Level.PartTwo)]
    private void Should_give_correct_answer(string input, string expectedAnswer, Level level)
    {
        var actual_answer = Solution.Solve(input, level);
        Assert.Equal(expectedAnswer, actual_answer);
    }
}

public static class Solution
{
    private delegate long ParsingStrategy(string input);

    public static string Solve(string input, Level level)
    {
        return SumAllCalibrationValues(input);

        string SumAllCalibrationValues(string input)
        {
            return input
                .Split("\n")
                .Select(line => ParseCalibrationValue(line, level))
                .Sum()
                .ToString();
        }

        static long ParseCalibrationValue(string line, Level level = Level.PartOne)
        {
            var parsingStrategy = ChooseStrategy(level);

            static ParsingStrategy ChooseStrategy(Level level) =>
                level switch
                {
                    Level.PartOne => ParseAsSingleDigitCharacters,
                    Level.PartTwo => ParseAsRegex,
                    _ => throw new ArgumentException("Cannot decide parsing strategy for level: " + level)
                };

            return parsingStrategy.Invoke(line);

            static long ParseAsSingleDigitCharacters(string line)
            {
                var first = line.First(char.IsDigit);
                var last = line.Last(char.IsDigit);
                var digits = $"{first}{last}";
                return long.Parse(digits);
            }

            static long ParseAsRegex(string line)
            {
                const string pattern = @"(?=(one|two|three|four|five|six|seven|eight|nine|\d)).";
                var matches = Regex.Matches(line, pattern, RegexOptions.IgnoreCase);
                var first = matches.First().Groups[^1].Value;
                var last = matches.Last().Groups[^1].Value;
                var digits = $"{DigitLiteralToNumber(first)}{DigitLiteralToNumber(last)}";
                return long.Parse(digits);
            }
        }

        static string DigitLiteralToNumber(string literal)
            =>
                literal switch
                {
                    "one" => "1",
                    "two" => "2",
                    "three" => "3",
                    "four" => "4",
                    "five" => "5",
                    "six" => "6",
                    "seven" => "7",
                    "eight" => "8",
                    "nine" => "9",
                    "zero" => "0",
                    _ => literal
                };
    }
}