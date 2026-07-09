using System.CommandLine;
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
        new(["--level", "-l"], () => Level.PartOne, description);

    public static Option<bool> Force(string description) =>
        new(["--force", "-f"], () => false, description);
}
