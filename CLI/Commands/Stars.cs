using System.CommandLine;
using System.CommandLine.Invocation;
using Engine;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using static Common.Constants;
using static Common.PathsProvider;

namespace CLI;

public class StarsCommand : Command
{
    public StarsCommand() : base("stars", "Show Advent of Code stars from the authenticated account")
    {
        AddOption(PuzzleCommandOptions.OptionalYear("Year to inspect."));
        AddOption(PuzzleCommandOptions.OptionalDay("Day to inspect."));
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ILogger<Handler> _logger;

        public int? Year { get; set; }
        public int? Day { get; set; }

        public Handler(IPuzzleEngine puzzleEngine, ILogger<Handler> logger)
        {
            _puzzleEngine = puzzleEngine;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                if (Day is < 1 or > 25)
                {
                    AnsiConsole.MarkupLine("[red]Error: --day must be between 1 and 25.[/]");
                    return 1;
                }

                if (!EnsureCookieExists())
                    return 1;

                if (!Year.HasValue && !Day.HasValue)
                    return await RenderEventStars();

                var year = Year ?? PuzzleCommandDefaults.Resolve(null, null).year;
                return await RenderCalendarStars(year, Day);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Advent of Code stars");
                AnsiConsole.WriteException(ex);
                return 1;
            }
        }

        private async Task<int> RenderEventStars()
        {
            var status = await AnsiConsole.Status()
                .StartAsync("Fetching Advent of Code events...", async _ =>
                    await _puzzleEngine.GetRemoteEventStatus());

            var table = new Table()
                .Title($"Advent of Code stars ({status.TotalStars}/{status.TotalAvailableStars})")
                .RoundedBorder();

            table.AddColumn("Year");
            table.AddColumn("Stars");

            foreach (var item in status.Events)
            {
                table.AddRow(
                    item.Year.ToString(),
                    FormatStarRatio(item.Stars, item.AvailableStars));
            }

            AnsiConsole.Write(table);
            return 0;
        }

        private async Task<int> RenderCalendarStars(int year, int? day)
        {
            var statuses = await AnsiConsole.Status()
                .StartAsync($"Fetching Advent of Code {year} calendar...", async _ =>
                    await _puzzleEngine.GetRemoteStatus(year));

            var rows = BuildRows(statuses, day);
            if (rows.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]No stars found for {year}.[/]");
                return 0;
            }

            var table = new Table()
                .Title($"Advent of Code {year} stars")
                .RoundedBorder();

            table.AddColumn("Year");
            table.AddColumn("Day");
            table.AddColumn("Stars");

            foreach (var row in rows)
            {
                table.AddRow(
                    year.ToString(),
                    row.Day.ToString("D2"),
                    FormatStarRatio(row.Stars, DayStarCapacity));
            }

            AnsiConsole.Write(table);
            return 0;
        }

        private static IReadOnlyList<RemotePuzzleStatus> BuildRows(
            IReadOnlyList<RemotePuzzleStatus> statuses,
            int? day)
        {
            if (day.HasValue)
            {
                var status = statuses.FirstOrDefault(item => item.Day == day.Value);
                return [status ?? new RemotePuzzleStatus(day.Value, 0)];
            }

            return statuses
                .OrderBy(item => item.Day)
                .ToArray();
        }

        private static bool EnsureCookieExists()
        {
            if (File.Exists(GetCookieFilePath()))
                return true;

            AnsiConsole.MarkupLine("[red]Error: Session cookie not found.[/]");
            AnsiConsole.MarkupLine("Please run [yellow]aoc config --cookie <value>[/] first.");
            return false;
        }

        private static string FormatStarRatio(int stars, int availableStars)
        {
            return stars switch
            {
                0 => $"[grey]0/{availableStars}[/]",
                var value when value >= availableStars => $"[green]{stars}/{availableStars}[/]",
                _ => $"[yellow]{stars}/{availableStars}[/]"
            };
        }

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}
