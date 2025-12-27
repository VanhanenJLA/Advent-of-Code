using System.CommandLine;
using System.CommandLine.Invocation;
using Engine;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Common;

namespace CLI;

public class SubmitCommand : Command
{
    public SubmitCommand() : base("submit", "Submit a puzzle answer")
    {
        var dayOption = new Option<int>(
            ["--day", "-d"],
            "Day of the puzzle.");
        AddOption(dayOption);

        var yearOption = new Option<int>(
            ["--year", "-y"],
            "Year of the puzzle.");
        AddOption(yearOption);

        var levelOption = new Option<Level>(
            ["--level", "-l"],
            () => Level.PartOne,
            "Level to submit (PartOne or PartTwo).");
        AddOption(levelOption);

        var answerArgument = new Argument<string>(
            "answer",
            "The answer to submit.");
        AddArgument(answerArgument);
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ILogger<Handler> _logger;
        public int? Day { get; set; }
        public int? Year { get; set; }
        public Level Level { get; set; }
        public required string Answer { get; set; }

        public Handler(IPuzzleEngine puzzleEngine, ILogger<Handler> logger)
        {
            _puzzleEngine = puzzleEngine;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                // TODO: Go over all default datetime assumptions like below and make them robust.
                var now = DateTime.Now;
                Year ??= now.Month == 12 ? now.Year : now.Year - 1;
                Day  ??= now.Month == 12 ? now.Day  : 1;

                var options = (Year.Value, Day.Value);

                await AnsiConsole.Status()
                    .StartAsync("Preparing submission...", async ctx =>
                    {
                        ctx.Status("Building submission context...");
                        AnsiConsole.MarkupLine(
                            $"Submitting [yellow]{Level}[/] answer for [green]{Year}-{Day:D2}[/]: [bold]{Answer}[/]");

                        ctx.Status("Submitting answer to puzzle engine...");
                        var success = await _puzzleEngine.SubmitAnswer(Answer, options, Level);

                        if (!success)
                        {
                            ctx.Status("Answer rejected.");
                            AnsiConsole.MarkupLine("[red]Incorrect answer. Try again![/]");
                        }

                        ctx.Status("Answer accepted. Awarding star...");
                        AnsiConsole.MarkupLine("[green]Correct! Star earned![/]");
                    });
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Unexpected error:[/] {ex.Message}");
                return -1;
            }

        }

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}
