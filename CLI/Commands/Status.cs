using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.RegularExpressions;
using Common;
using Engine;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using static Common.Constants;
using static Common.PathsProvider;

namespace CLI;

public class StatusCommand : Command
{
    public StatusCommand() : base("status", "Show local puzzle progress and optional remote Advent of Code stars")
    {
        AddOption(new Option<int?>(["--year", "-y"], "Year to inspect."));
        AddOption(new Option<int?>(["--day", "-d"], "Day to inspect."));
        AddOption(new Option<bool>(["--remote", "-r"], () => false, "Fetch Advent of Code calendar status."));
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ILogger<Handler> _logger;

        public int? Year { get; set; }
        public int? Day { get; set; }
        public bool Remote { get; set; }

        public Handler(IPuzzleEngine puzzleEngine, ILogger<Handler> logger)
        {
            _puzzleEngine = puzzleEngine;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                var year = Year ?? PuzzleCommandDefaults.Resolve(null, null).year;
                if (Day is < 1 or > 25)
                {
                    AnsiConsole.MarkupLine("[red]Error: --day must be between 1 and 25.[/]");
                    return 1;
                }

                var localStatuses = LocalPuzzleStatusReader.Read(year, Day);
                var remoteStatuses = await GetRemoteStatuses(year);
                if (remoteStatuses is null)
                    return 1;

                var rows = BuildRows(year, Day, localStatuses, remoteStatuses);
                if (rows.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]No local solutions found for {year}.[/]");
                    return 0;
                }

                Render(year, rows);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading puzzle status");
                AnsiConsole.WriteException(ex);
                return 1;
            }
        }

        private async Task<IReadOnlyDictionary<int, int>?> GetRemoteStatuses(int year)
        {
            if (!Remote)
                return new Dictionary<int, int>();

            var cookiePath = GetCookieFilePath();
            if (!File.Exists(cookiePath))
            {
                AnsiConsole.MarkupLine("[red]Error: Session cookie not found.[/]");
                AnsiConsole.MarkupLine("Please run [yellow]aoc config --cookie <value>[/] first.");
                return null;
            }

            var statuses = await AnsiConsole.Status()
                .StartAsync($"Fetching Advent of Code {year} calendar...", async _ =>
                    await _puzzleEngine.GetRemoteStatus(year));

            return statuses.ToDictionary(status => status.Day, status => status.Stars);
        }

        private IReadOnlyList<PuzzleStatusRow> BuildRows(
            int year,
            int? day,
            IReadOnlyList<LocalPuzzleStatus> localStatuses,
            IReadOnlyDictionary<int, int> remoteStatuses)
        {
            var localByDay = localStatuses.ToDictionary(status => status.Day);
            var days = new SortedSet<int>();

            if (day.HasValue)
            {
                days.Add(day.Value);
            }
            else
            {
                foreach (var status in localStatuses)
                    days.Add(status.Day);

                if (Remote)
                {
                    foreach (var remoteDay in remoteStatuses.Keys)
                        days.Add(remoteDay);
                }
            }

            return days
                .Select(currentDay =>
                {
                    var local = localByDay.GetValueOrDefault(currentDay) ?? LocalPuzzleStatus.Missing(currentDay);
                    var remoteStars = Remote ? remoteStatuses.GetValueOrDefault(currentDay, 0) : (int?)null;
                    return new PuzzleStatusRow(year, currentDay, local, remoteStars);
                })
                .ToArray();
        }

        private void Render(int year, IReadOnlyList<PuzzleStatusRow> rows)
        {
            var table = new Table()
                .Title($"Advent of Code {year} status")
                .RoundedBorder();

            table.AddColumn("Year");
            table.AddColumn("Day");
            if (Remote)
                table.AddColumn("Remote");
            table.AddColumn("Soln");
            table.AddColumn("Input");
            table.AddColumn("Instr");
            table.AddColumn("P1");
            table.AddColumn("P2");
            table.AddColumn("Notes");

            foreach (var row in rows)
            {
                var cells = new List<string>
                {
                    row.Year.ToString(),
                    row.Day.ToString("D2")
                };

                if (Remote)
                    cells.Add(FormatRemoteStars(row.RemoteStars ?? 0));

                cells.Add(FormatBool(row.Local.HasSolution));
                cells.Add(FormatBool(row.Local.HasInput));
                cells.Add(FormatBool(row.Local.HasInstructions));
                cells.Add(FormatPart(row.Local.PartOne));
                cells.Add(FormatPart(row.Local.PartTwo));
                cells.Add(Markup.Escape(BuildNotes(row)));

                table.AddRow(cells.ToArray());
            }

            AnsiConsole.Write(table);
        }

        private static string BuildNotes(PuzzleStatusRow row)
        {
            var notes = row.Local.Notes.ToList();

            if (row.RemoteStars >= 1 && row.Local.PartOne != LocalPartStatus.AnswerRecorded)
                notes.Add("P1 answer missing");

            if (row.RemoteStars >= 2 && row.Local.PartTwo != LocalPartStatus.AnswerRecorded)
                notes.Add("P2 answer missing");

            return string.Join("; ", notes);
        }

        private static string FormatRemoteStars(int stars) => stars switch
        {
            0 => "[grey]0/2[/]",
            1 => "[yellow]1/2[/]",
            _ => "[green]2/2[/]"
        };

        private static string FormatBool(bool value) => value
            ? "[green]yes[/]"
            : "[red]no[/]";

        private static string FormatPart(LocalPartStatus status) => status switch
        {
            LocalPartStatus.AnswerRecorded => "[green]answer[/]",
            LocalPartStatus.ExampleOnly => "[yellow]example[/]",
            LocalPartStatus.Todo => "[yellow]TBD[/]",
            LocalPartStatus.Commented => "[yellow]commented[/]",
            LocalPartStatus.NotImplemented => "[red]not implemented[/]",
            _ => "[grey]missing[/]"
        };

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}

internal record PuzzleStatusRow(int Year, int Day, LocalPuzzleStatus Local, int? RemoteStars);

internal record LocalPuzzleStatus(
    int Day,
    bool HasSolution,
    bool HasInput,
    bool HasInstructions,
    LocalPartStatus PartOne,
    LocalPartStatus PartTwo,
    IReadOnlyList<string> Notes)
{
    public static LocalPuzzleStatus Missing(int day) => new(
        day,
        HasSolution: false,
        HasInput: false,
        HasInstructions: false,
        LocalPartStatus.Missing,
        LocalPartStatus.Missing,
        Array.Empty<string>());
}

internal enum LocalPartStatus
{
    Missing,
    Commented,
    Todo,
    ExampleOnly,
    AnswerRecorded,
    NotImplemented
}

internal static class LocalPuzzleStatusReader
{
    private static readonly Regex RealAnswerPattern = new(
        @"InlineData\s*\(\s*null\s*,\s*""(?!TBD"")[^""]+""",
        RegexOptions.Compiled | RegexOptions.Singleline);

    public static IReadOnlyList<LocalPuzzleStatus> Read(int year, int? day)
    {
        if (day.HasValue)
            return [ReadDay(year, day.Value)];

        var yearDirectory = Path.Combine(GetSolutionsProjectRootDirectory(), year.ToString());
        if (!Directory.Exists(yearDirectory))
            return Array.Empty<LocalPuzzleStatus>();

        return Directory
            .EnumerateDirectories(yearDirectory)
            .Select(directory => (Directory: directory, Name: Path.GetFileName(directory)))
            .Where(item => int.TryParse(item.Name, out _))
            .Select(item => int.Parse(item.Name))
            .OrderBy(currentDay => currentDay)
            .Select(currentDay => ReadDay(year, currentDay))
            .ToArray();
    }

    private static LocalPuzzleStatus ReadDay(int year, int day)
    {
        var directory = Path.Combine(GetSolutionsProjectRootDirectory(), year.ToString(), day.ToString());
        var solutionPath = Path.Combine(directory, SolutionFileName);
        var hasSolution = File.Exists(solutionPath);
        var hasInput = File.Exists(Path.Combine(directory, InputTextFileName));
        var hasInstructions = File.Exists(Path.Combine(directory, InstructionsFilename));

        if (!hasSolution)
            return new LocalPuzzleStatus(
                day,
                HasSolution: false,
                hasInput,
                hasInstructions,
                LocalPartStatus.Missing,
                LocalPartStatus.Missing,
                Array.Empty<string>());

        var content = File.ReadAllText(solutionPath);
        var isNotImplemented = content.Contains("throw new NotImplementedException", StringComparison.Ordinal);
        var partOne = isNotImplemented ? LocalPartStatus.NotImplemented : InferPartStatus(content, Level.PartOne);
        var partTwo = isNotImplemented ? LocalPartStatus.NotImplemented : InferPartStatus(content, Level.PartTwo);
        var notes = BuildLocalNotes(partOne, partTwo);

        return new LocalPuzzleStatus(day, hasSolution, hasInput, hasInstructions, partOne, partTwo, notes);
    }

    private static IReadOnlyList<string> BuildLocalNotes(LocalPartStatus partOne, LocalPartStatus partTwo)
    {
        var notes = new List<string>();

        if (partOne == LocalPartStatus.Commented)
            notes.Add("P1 commented");

        if (partTwo == LocalPartStatus.Commented)
            notes.Add("P2 commented");

        return notes;
    }

    private static LocalPartStatus InferPartStatus(string content, Level level)
    {
        var activeAttributes = ReadInlineDataAttributes(content, commented: false)
            .Where(attribute => attribute.Contains($"Level.{level}", StringComparison.Ordinal))
            .ToArray();

        if (activeAttributes.Any(IsRealAnswer))
            return LocalPartStatus.AnswerRecorded;

        if (activeAttributes.Any(attribute => !IsTbd(attribute)))
            return LocalPartStatus.ExampleOnly;

        if (activeAttributes.Any(IsTbd))
            return LocalPartStatus.Todo;

        var commentedAttributes = ReadInlineDataAttributes(content, commented: true)
            .Where(attribute => attribute.Contains($"Level.{level}", StringComparison.Ordinal))
            .ToArray();

        return commentedAttributes.Any()
            ? LocalPartStatus.Commented
            : LocalPartStatus.Missing;
    }

    private static IReadOnlyList<string> ReadInlineDataAttributes(string content, bool commented)
    {
        var attributes = new List<string>();
        var lines = content.ReplaceLineEndings("\n").Split('\n');

        for (var index = 0; index < lines.Length; index++)
        {
            var trimmed = lines[index].TrimStart();
            var isCommented = trimmed.StartsWith("//", StringComparison.Ordinal);
            var candidate = isCommented ? trimmed[2..].TrimStart() : trimmed;

            if (isCommented != commented || !candidate.StartsWith("[InlineData(", StringComparison.Ordinal))
                continue;

            while (!candidate.Contains(")]", StringComparison.Ordinal) && index + 1 < lines.Length)
            {
                index++;
                candidate += " " + lines[index].Trim();
            }

            attributes.Add(candidate);
        }

        return attributes;
    }

    private static bool IsRealAnswer(string attribute) => RealAnswerPattern.IsMatch(attribute);

    private static bool IsTbd(string attribute) => attribute.Contains("\"TBD\"", StringComparison.Ordinal);
}
