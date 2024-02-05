using System.Text.RegularExpressions;
using Common;

namespace Advent_of_Code._2023._1;

public class Tests : TestBase
{
    [Theory]
    [InlineData("1abc2\npqr3stu8vwx\na1b2c3d4e5f\ntreb7uchet", "142", Level.PartOne)]
    [InlineData(null, "54990", Level.PartOne)]

    [InlineData("two1nine\neightwothree\nabcone2threexyz\nxtwone3four\n4nineeightseven2\nzoneight234\n7pqrstsixteen",
        "281", Level.PartTwo)]
    [InlineData("6zfxp", "66", Level.PartTwo)]
    [InlineData("oneight", "18", Level.PartTwo)]
    [InlineData("twone", "21", Level.PartTwo)]
    [InlineData(null, "54473", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }
}

public class Solution : ISolution
{
    private delegate long ParsingStrategy(string input);
    
    public string Solve(string input, Level level)
    {
     
        var parsingStrategy = Get(level);
        
        return input
            .Split("\n")
            .Select(ParseCalibrationValue)
            .Sum()
            .ToString();

        long ParseCalibrationValue(string line) => parsingStrategy(line);
        
    }
    
    static ParsingStrategy Get(Level level) =>
        level switch
        {
            Level.PartOne => ParseAsSingleDigitCharacters,
            Level.PartTwo => ParseAsRegex,
            _ => throw new ArgumentException("Cannot decide parsing strategy for level: " + level)
        };

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
        var first = matches[0].Groups[^1].Value;
        var last = matches[^1].Groups[^1].Value;
        var firstDigit = ConvertWrittenNumberToSingleDigit(first);
        var lastDigit = ConvertWrittenNumberToSingleDigit(last);
        var digits = $"{firstDigit}{lastDigit}";
        return long.Parse(digits);
    }
    
    static string ConvertWrittenNumberToSingleDigit(string literal)
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
                _ => literal // Naively assume that an unmatched literal is already in digit format. I.e. "7"
            };
}