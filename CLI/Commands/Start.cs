using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Engine;

namespace CLI;

public class StartCommand : Command
{
    public StartCommand() : base("start", "Start solving a new puzzle (scaffold, fetch input, fetch instructions)")
    {
        var dayOption = new Option<int>(
            ["--day", "-d"],
            "Day to start.");
        AddOption(dayOption);
        
        var yearOption = new Option<int>(
            ["--year", "-y"],
            "Year to start.");
        AddOption(yearOption);
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ILogger<Handler> _logger;
        public int? Day { get; set; }
        public int? Year { get; set; }

        public Handler(IPuzzleEngine puzzleEngine, ILogger<Handler> logger)
        {
            _puzzleEngine = puzzleEngine;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                var cookiePath = Common.PathsProvider.GetCookieFilePath();
                if (!File.Exists(cookiePath))
                {
                    AnsiConsole.MarkupLine("[red]Error: Session cookie not found.[/]");
                    AnsiConsole.MarkupLine("Please run [yellow]aoc config --session <value>[/] first.");
                    return 1;
                }

                Year ??= 2020;
                Day ??= 20;
                // Maybe have some sensible defaults such as initialize current day if not provided and christmas.
                // However got to remember to handle edge case like local time vs. aoc time and puzzle not yet released.

                var options = (Year.Value, Day.Value);

                await AnsiConsole.Status()
                    .StartAsync("Starting puzzle...", async ctx =>
                    {
                        ctx.Status("Scaffolding solution...");
                        await _puzzleEngine.Start(options);
                        
                        ctx.Status("Fetching puzzle input...");
                        var input = await _puzzleEngine.GetInput(options);
                        
                        ctx.Status("Fetching puzzle instructions...");
                        var content = await _puzzleEngine.GetInstructions(options);
                    });

                AnsiConsole.MarkupLine($"[green]Successfully started puzzle for {Year}-{Day:D2}![/]");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting puzzle");
                AnsiConsole.WriteException(ex);
                return 1;
            }
        }

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}
