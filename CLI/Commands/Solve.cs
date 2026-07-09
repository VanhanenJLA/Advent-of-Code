using System.CommandLine;
using System.CommandLine.Invocation;
using Common;
using Engine;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace CLI;

public class SolveCommand : Command
{
    public SolveCommand() : base("solve", "Solve a puzzle using the locally implemented solution and real input")
    {
        AddOption(PuzzleCommandOptions.OptionalDay("Day of the puzzle."));
        AddOption(PuzzleCommandOptions.OptionalYear("Year of the puzzle."));
        AddOption(PuzzleCommandOptions.PuzzleLevel("Part to solve (1, 2, PartOne, or PartTwo)."));
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ISolutionResolver _solutionResolver;
        private readonly ILogger<Handler> _logger;

        public int? Day { get; set; }
        public int? Year { get; set; }
        public Level Level { get; set; }

        public Handler(
            IPuzzleEngine puzzleEngine,
            ISolutionResolver solutionResolver,
            ILogger<Handler> logger)
        {
            _puzzleEngine = puzzleEngine;
            _solutionResolver = solutionResolver;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                var options = PuzzleCommandDefaults.Resolve(Year, Day);
                var answer = await Solve(options);

                AnsiConsole.MarkupLine(
                    $"[green]{options.year}-{options.day:D2} {Level}:[/] [bold]{answer}[/]");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error solving puzzle");
                AnsiConsole.WriteException(ex);
                return 1;
            }
        }

        private async Task<string> Solve((int year, int day) options)
        {
            var solution = _solutionResolver.Resolve(options.year, options.day);
            var input = await _puzzleEngine.GetInput(options);
            return solution.Solve(input, Level);
        }

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}

public class SolveSubmitCommand : Command
{
    public SolveSubmitCommand() : base("solve-submit", "Solve a puzzle using real input and submit the computed answer")
    {
        AddOption(PuzzleCommandOptions.OptionalDay("Day of the puzzle."));
        AddOption(PuzzleCommandOptions.OptionalYear("Year of the puzzle."));
        AddOption(PuzzleCommandOptions.PuzzleLevel("Part to submit (1, 2, PartOne, or PartTwo)."));
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ISolutionResolver _solutionResolver;
        private readonly ILogger<Handler> _logger;

        public int? Day { get; set; }
        public int? Year { get; set; }
        public Level Level { get; set; }

        public Handler(
            IPuzzleEngine puzzleEngine,
            ISolutionResolver solutionResolver,
            ILogger<Handler> logger)
        {
            _puzzleEngine = puzzleEngine;
            _solutionResolver = solutionResolver;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                var options = PuzzleCommandDefaults.Resolve(Year, Day);
                var solution = _solutionResolver.Resolve(options.year, options.day);
                var input = await _puzzleEngine.GetInput(options);
                var answer = solution.Solve(input, Level);

                AnsiConsole.MarkupLine(
                    $"Submitting [yellow]{Level}[/] answer for [green]{options.year}-{options.day:D2}[/]: [bold]{answer}[/]");

                var success = await _puzzleEngine.SubmitAnswer(answer, options, Level);

                if (!success)
                {
                    AnsiConsole.MarkupLine("[red]Incorrect answer. Try again![/]");
                    return 1;
                }

                AnsiConsole.MarkupLine("[green]Correct! Star earned![/]");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error solving and submitting puzzle");
                AnsiConsole.WriteException(ex);
                return 1;
            }
        }

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}

internal static class PuzzleCommandDefaults
{
    public static (int year, int day) Resolve(int? year, int? day)
    {
        var now = DateTime.Now;
        return (
            year ?? (now.Month == 12 ? now.Year : now.Year - 1),
            day ?? (now.Month == 12 ? now.Day : 1));
    }
}
