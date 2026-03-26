using System.CommandLine;
using System.CommandLine.Invocation;
using Engine;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace CLI;

public class SyncCommand : Command
{
    public SyncCommand() : base("sync", "Sync puzzle inputs and instructions for all existing solutions in a year")
    {
        var yearOption = new Option<int?>(
            ["--year", "-y"],
            "Year to sync.");
        AddOption(yearOption);

        var forceOption = new Option<bool>(
            ["--force", "-f"],
            () => false,
            "Ignore cached files and retrieve fresh data from the API.");
        AddOption(forceOption);
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ILogger<Handler> _logger;
        public int? Year { get; set; }
        public bool Force { get; set; }

        public Handler(IPuzzleEngine puzzleEngine, ILogger<Handler> logger)
        {
            _puzzleEngine = puzzleEngine;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            if (!Year.HasValue)
            {
                AnsiConsole.MarkupLine("[red]Error: --year is required.[/]");
                return 1;
            }

            try
            {
                var syncedDays = await AnsiConsole.Status()
                    .StartAsync($"Syncing puzzle files for {Year.Value}...", async _ =>
                        await _puzzleEngine.SyncYear(Year.Value, Force));

                if (syncedDays.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]No existing solutions found for {Year.Value}.[/]");
                    return 0;
                }

                var formattedDays = string.Join(", ", syncedDays.Select(day => day.ToString("D2")));
                AnsiConsole.MarkupLine(
                    $"[green]Synced puzzle files for {Year.Value}:[/] {formattedDays}");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing puzzle files");
                AnsiConsole.WriteException(ex);
                return 1;
            }
        }

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}
