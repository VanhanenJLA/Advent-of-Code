using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Engine;

namespace CLI;

public class RemoveCommand : Command
{
    public RemoveCommand() : base("remove", "Remove solution files for a puzzle (unstart)")
    {
        AddAlias("unstart");
        
        var dayOption = new Option<int>(
            ["--day", "-d"],
            "Day to remove.");
        AddOption(dayOption);
        
        var yearOption = new Option<int>(
            ["--year", "-y"],
            "Year to remove.");
        AddOption(yearOption);

        var forceOption = new Option<bool>(
            ["--force", "-f"],
            "Bypass confirmation prompt.");
        AddOption(forceOption);
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ILogger<Handler> _logger;
        public int? Day { get; set; }
        public int? Year { get; set; }
        public bool Force { get; set; }

        public Handler(IPuzzleEngine puzzleEngine, ILogger<Handler> logger)
        {
            _puzzleEngine = puzzleEngine;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                Year ??= 2020;
                Day ??= 20;

                if (!Force)
                {
                    if (!AnsiConsole.Confirm($"Are you sure you want to [red]remove[/] the solution for {Year}-{Day:D2}?"))
                    {
                        AnsiConsole.MarkupLine("[yellow]Aborted.[/]");
                        return 0;
                    }
                }

                var options = (Year.Value, Day.Value);
                var success = await _puzzleEngine.Unstart(options);

                if (success)
                {
                    AnsiConsole.MarkupLine($"[green]Successfully removed solution for {Year}-{Day:D2}![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[yellow]No solution found for {Year}-{Day:D2} to remove.[/]");
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing puzzle");
                AnsiConsole.WriteException(ex);
                return 1;
            }
        }

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}
