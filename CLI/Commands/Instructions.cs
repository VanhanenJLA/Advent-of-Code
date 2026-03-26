using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Engine;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace CLI;

public class InstructionsCommand : Command
{
    public InstructionsCommand() : base("instructions", "Interact with instructions API")
    {
        AddCommand(new GetInstructionCommand());
    }
}

public class GetInstructionCommand : Command
{
    public GetInstructionCommand() : base("get", "Retrieve instructions for a given date's puzzle")
    {

        var dayOption = new Option<int>(
            ["--day", "-d"],
            "Day to retrieve.");
        AddOption(dayOption);
        
        var yearOption = new Option<int>(
            ["--year", "-y"],
            "Year to retrieve.");
        AddOption(yearOption);
        
        var forceOption = new Option<bool>(
            ["--force", "-f"],
            () => false,
            "Ignore cached instructions and retrieve fresh data from the API.");
        AddOption(forceOption);
        
        // (The global --json option is automatically available from the root command)
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ILogger<Handler> _logger;
        public int Day { get; set; }
        public int? Year { get; set; }
        public bool Force { get; set; }
        public bool Json { get; set; }

        public Handler(IPuzzleEngine puzzleEngine, ILogger<Handler> logger)
        {
            _puzzleEngine = puzzleEngine;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                Year ??= DateOnly.FromDateTime(DateTime.Now).Year - 1;
                var instructions = await _puzzleEngine.GetInstructions((Year.Value, Day), Force);
                var articles = _puzzleEngine.ParseInstructions(instructions);
                foreach (var a in articles)
                {
                    HtmlToSpectreConverter.RenderHtmlToConsole(a.OuterHtml);
                }
                // Output(markup);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from API");
                AnsiConsole.WriteException(ex);
                return 1;
            }
        }

        private void Output(object data)
        {
            if (Json)
            {
                // Serialize the data object to JSON
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                AnsiConsole.WriteLine(json);
            }
            else
            {
                // Default text output (could be simplified or formatted as needed)
                AnsiConsole.WriteLine(data?.ToString() ?? string.Empty);
            }
        }

        // ICommandHandler also requires implementing the synchronous Invoke, which can delegate to InvokeAsync
        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}