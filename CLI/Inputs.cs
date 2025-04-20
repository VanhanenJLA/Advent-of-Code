using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using API;
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
        private readonly AdventOfCodeAPI _api;
        private readonly ILogger<Handler> _logger;
        public int Day { get; set; }
        public int? Year { get; set; }
        public bool Json { get; set; }

        public Handler(AdventOfCodeAPI api, ILogger<Handler> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                Year ??= DateOnly.FromDateTime(DateTime.Now).Year - 1;
                var item = await _api.GetInput((Year.Value, Day));
                Output(item);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from API");
                await Console.Error.WriteLineAsync($"Failed to retrieve data: {ex.Message}");
                return 1;
            }
        }

        private void Output(object data)
        {
            if (Json)
            {
                // Serialize the data object to JSON
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
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