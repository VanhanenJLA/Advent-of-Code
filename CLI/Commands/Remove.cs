using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Engine;

namespace CLI;

public class RemoveCommand : Command
{
    public RemoveCommand() : base("remove", "Remove solution files for a puzzle or an entire year")
    {
        AddAlias("unstart");
        
        var dayOption = new Option<int?>(
            ["--day", "-d"],
            "Day to remove. If omitted, removes the entire year.");
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
                var target = Day.HasValue
                    ? $"{Year}-{Day.Value:D2}"
                    : $"the entire year {Year}";

                if (!Force)
                {
                    if (!AnsiConsole.Confirm($"Are you sure you want to [red]remove[/] {target}?"))
                    {
                        AnsiConsole.MarkupLine("[yellow]Aborted.[/]");
                        return 0;
                    }
                }

                var options = (Year.Value, Day);
                var success = await _puzzleEngine.Unstart(options);

                if (success)
                {
                    AnsiConsole.MarkupLine($"[green]Successfully removed {target}![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[yellow]No solution found for {target} to remove.[/]");
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
