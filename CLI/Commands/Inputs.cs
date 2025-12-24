using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Engine;
using Microsoft.Extensions.Logging;

namespace CLI;

public class InputsCommand : Command
{
    public InputsCommand() : base("inputs", "Fetch puzzle inputs from API")
    {
        AddCommand(new GetInputCommand());
    }
}

public class GetInputCommand : Command
{
    public GetInputCommand() : base("get", "Retrieve personal puzzle input")
    {

        var dayOption = new Option<int>(
            ["--day", "-d"],
            "Day to retrieve.");
        AddOption(dayOption);
        
        var yearOption = new Option<int>(
            ["--year", "-y"],
            "Year to retrieve.");
        AddOption(yearOption);
        
        // (The global --json option is automatically available from the root command)
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ILogger<Handler> _logger;
        private int? Day { get; set; }
        private int? Year { get; set; }
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
                Year ??= 2020;
                Day ??= 20;
                var input = await _puzzleEngine.GetInput((Year.Value, Day.Value));
                Output(input);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving puzzle input");
                return 1;
            }
        }

        private void Output(object data)
        {
            if (Json)
            {
                // Serialize the data object to JSON
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(json);
            }
            else
            {
                // Default text output (could be simplified or formatted as needed)
                Console.WriteLine(data?.ToString());
            }
        }

        // ICommandHandler also requires implementing the synchronous Invoke, which can delegate to InvokeAsync
        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}