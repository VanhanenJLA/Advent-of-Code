using System.CommandLine;
using System.CommandLine.Invocation;
using Engine.Integrations;
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
        private readonly AdventOfCodeAPI _api;
        private readonly ILogger<Handler> _logger;
        public int Day { get; set; }
        public int? Year { get; set; }

        public Handler(AdventOfCodeAPI api, ILogger<Handler> logger)
        {
            _api = api;
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

                Year ??= DateOnly.FromDateTime(DateTime.Now).Year;
                if (Day == 0) Day = DateTime.Now.Day;

                var options = (Year.Value, Day);

                await AnsiConsole.Status()
                    .StartAsync("Starting puzzle...", async ctx =>
                    {
                        ctx.Status("Scaffolding solution...");
                        await PuzzleEngine.CreateSolution(options);
                        
                        ctx.Status("Fetching puzzle input...");
                        var input = await _api.GetInput(options);
                        await PuzzleEngine.SaveInput(input, options);
                        
                        ctx.Status("Fetching puzzle instructions...");
                        var content = await _api.GetInstructions(options);
                        var instructions = PuzzleEngine.ParseInstructions(content);
                        PuzzleEngine.SaveInstructions(options, instructions);
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
