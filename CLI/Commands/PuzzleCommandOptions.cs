using System.CommandLine;
using System.CommandLine.Parsing;
using Common;

namespace CLI;

internal static class PuzzleCommandOptions
{
    public static Option<int> RequiredDay(string description) =>
        new(["--day", "-d"], description);

    public static Option<int?> OptionalDay(string description) =>
        new(["--day", "-d"], description);

    public static Option<int> RequiredYear(string description) =>
        new(["--year", "-y"], description);

    public static Option<int?> OptionalYear(string description) =>
        new(["--year", "-y"], description);

    public static Option<Level> PuzzleLevel(string description) =>
        new(["--level", "-l", "--part"], ParseLevel, isDefault: true, description);

    public static Option<bool> Force(string description) =>
        new(["--force", "-f"], () => false, description);

    private static Level ParseLevel(ArgumentResult result)
    {
        var value = result.Tokens.SingleOrDefault()?.Value;
        if (value is null)
            return Level.PartOne;

        if (TryParseLevel(value, out var level))
            return level;

        result.ErrorMessage = "Part must be PartOne, PartTwo, 1, or 2.";
        return Level.PartOne;
    }

    private static bool TryParseLevel(string value, out Level level)
    {
        level = value.ToLowerInvariant() switch
        {
            "1" or "one" or "part1" or "partone" or "part-one" => Level.PartOne,
            "2" or "two" or "part2" or "parttwo" or "part-two" => Level.PartTwo,
            _ => default
        };

        if (level != default)
            return true;

        return Enum.TryParse(value, ignoreCase: true, out level)
               && Enum.IsDefined(typeof(Level), level);
    }
}
