using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.RegularExpressions;
using Common;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using static Common.Constants;
using static Common.PathsProvider;

namespace CLI;

public class StatusCommand : Command
{
    public StatusCommand() : base("status", "Show local repository puzzle status")
    {
        AddOption(PuzzleCommandOptions.OptionalYear("Year to inspect."));
        AddOption(PuzzleCommandOptions.OptionalDay("Day to inspect."));
    }

    public new class Handler : ICommandHandler
    {
        private readonly ILogger<Handler> _logger;

        public int? Year { get; set; }
        public int? Day { get; set; }

        public Handler(ILogger<Handler> logger)
        {
            _logger = logger;
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                if (Day is < 1 or > 25)
                {
                    AnsiConsole.MarkupLine("[red]Error: --day must be between 1 and 25.[/]");
                    return Task.FromResult(1);
                }

                if (!Year.HasValue && !Day.HasValue)
                    return Task.FromResult(RenderTopLevelStatus());

                var year = Year ?? PuzzleCommandDefaults.Resolve(null, null).year;
                var localStatuses = LocalPuzzleStatusReader.Read(year, Day);
                var rows = BuildRows(year, Day, localStatuses);
                if (rows.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]No local solutions found for {year}.[/]");
                    return Task.FromResult(0);
                }

                Render(year, rows);
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading puzzle status");
                AnsiConsole.WriteException(ex);
                return Task.FromResult(1);
            }
        }

        private int RenderTopLevelStatus()
        {
            var localStatusesByYear = LocalPuzzleStatusReader.ReadAllYears();
            var rows = localStatusesByYear
                .Select(item => LocalYearStatusRow.From(item.Key, item.Value))
                .OrderByDescending(row => row.Year)
                .ToArray();
            if (rows.Length == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No local solutions found.[/]");
                return 0;
            }

            RenderYearSummary(rows);
            return 0;
        }

        private void RenderYearSummary(IReadOnlyList<LocalYearStatusRow> rows)
        {
            var table = new Table()
                .Title("Advent of Code local status")
                .RoundedBorder();

            table.AddColumn("Year");
            table.AddColumn("Status");
            table.AddColumn("Solns");
            table.AddColumn("Input");
            table.AddColumn("Instr");
            table.AddColumn("Notes");

            foreach (var row in rows)
            {
                table.AddRow(
                    row.Year.ToString(),
                    Markup.Escape(row.Status),
                    row.SolutionDays.ToString(),
                    row.InputDays.ToString(),
                    row.InstructionDays.ToString(),
                    Markup.Escape(row.Notes));
            }

            AnsiConsole.Write(table);
        }

        private IReadOnlyList<PuzzleStatusRow> BuildRows(
            int year,
            int? day,
            IReadOnlyList<LocalPuzzleStatus> localStatuses)
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
            }

            return days
                .Select(currentDay =>
                {
                    var local = localByDay.GetValueOrDefault(currentDay) ?? LocalPuzzleStatus.Missing(currentDay);
                    return new PuzzleStatusRow(year, currentDay, local);
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

                cells.Add(FormatBool(row.Local.HasSolution));
                cells.Add(FormatBool(row.Local.HasInput));
                cells.Add(FormatBool(row.Local.HasInstructions));
                cells.Add(FormatPart(row.Local.PartOne));
                cells.Add(FormatPart(row.Local.PartTwo));
                cells.Add(Markup.Escape(string.Join("; ", row.Local.Notes)));

                table.AddRow(cells.ToArray());
            }

            AnsiConsole.Write(table);
        }

        private static string FormatBool(bool value) => value
            ? "[green]yes[/]"
            : "[red]no[/]";

        private static string FormatPart(LocalPartStatus status) => status switch
        {
            LocalPartStatus.AnswerRecorded => "[green]recorded[/]",
            LocalPartStatus.ExampleOnly => "[yellow]example[/]",
            LocalPartStatus.Todo => "[yellow]TBD[/]",
            LocalPartStatus.Commented => "[yellow]commented[/]",
            LocalPartStatus.NotImplemented => "[red]not implemented[/]",
            _ => "[grey]missing[/]"
        };

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}

internal record LocalYearStatusRow(
    int Year,
    string Status,
    int SolutionDays,
    int InputDays,
    int InstructionDays,
    string Notes)
{
    public static LocalYearStatusRow From(int year, IReadOnlyList<LocalPuzzleStatus> localStatuses)
    {
        var solutionDays = localStatuses.Count(status => status.HasSolution);
        var inputDays = localStatuses.Count(status => status.HasInput);
        var instructionDays = localStatuses.Count(status => status.HasInstructions);
        var status = InferStatus(localStatuses);
        var notes = BuildNotes(localStatuses);
        return new LocalYearStatusRow(
            year,
            status,
            solutionDays,
            inputDays,
            instructionDays,
            notes);
    }

    private static string InferStatus(IReadOnlyList<LocalPuzzleStatus> localStatuses)
    {
        if (localStatuses.Count == 0)
            return "none";

        var solutionStatuses = localStatuses
            .Where(status => status.HasSolution)
            .ToArray();

        if (solutionStatuses.Length == 0)
            return "cached";

        if (solutionStatuses.Any(status =>
                status.PartOne == LocalPartStatus.NotImplemented ||
                status.PartTwo == LocalPartStatus.NotImplemented))
            return "not implemented";

        if (solutionStatuses.Any(status =>
                status.PartOne is LocalPartStatus.Todo or LocalPartStatus.Commented or LocalPartStatus.Missing ||
                status.PartTwo is LocalPartStatus.Todo or LocalPartStatus.Commented or LocalPartStatus.Missing))
            return "in progress";

        return "recorded";
    }

    private static string BuildNotes(IReadOnlyList<LocalPuzzleStatus> localStatuses)
    {
        var solutionStatuses = localStatuses
            .Where(status => status.HasSolution)
            .ToArray();
        var todoParts = CountParts(solutionStatuses, LocalPartStatus.Todo);
        var commentedParts = CountParts(solutionStatuses, LocalPartStatus.Commented);
        var missingParts = CountParts(solutionStatuses, LocalPartStatus.Missing);
        var noSolutionDays = localStatuses.Count(status => !status.HasSolution);
        var notImplementedDays = solutionStatuses.Count(status =>
            status.PartOne == LocalPartStatus.NotImplemented ||
            status.PartTwo == LocalPartStatus.NotImplemented);

        var notes = new List<string>();
        if (noSolutionDays > 0)
            notes.Add($"{noSolutionDays} no soln");

        if (todoParts > 0)
            notes.Add($"{todoParts} TBD");

        if (commentedParts > 0)
            notes.Add($"{commentedParts} commented");

        if (missingParts > 0)
            notes.Add($"{missingParts} missing");

        if (notImplementedDays > 0)
            notes.Add($"{notImplementedDays} not impl");

        return string.Join("; ", notes);
    }

    private static int CountParts(IReadOnlyList<LocalPuzzleStatus> localStatuses, LocalPartStatus status) =>
        localStatuses.Sum(local => Count(local.PartOne, status) + Count(local.PartTwo, status));

    private static int Count(LocalPartStatus actual, LocalPartStatus expected) =>
        actual == expected ? 1 : 0;
}

internal record PuzzleStatusRow(int Year, int Day, LocalPuzzleStatus Local);

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

    public static IReadOnlyDictionary<int, IReadOnlyList<LocalPuzzleStatus>> ReadAllYears()
    {
        var solutionsRoot = GetSolutionsProjectRootDirectory();
        if (!Directory.Exists(solutionsRoot))
            return new Dictionary<int, IReadOnlyList<LocalPuzzleStatus>>();

        return Directory
            .EnumerateDirectories(solutionsRoot)
            .Select(directory => Path.GetFileName(directory))
            .Where(name => int.TryParse(name, out _))
            .Select(name => int.Parse(name!))
            .OrderByDescending(year => year)
            .ToDictionary(year => year, year => Read(year, null));
    }

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
