using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;

namespace CLI.Commands;

public class ItemsCommand : Command
{
    public ItemsCommand() : base("items", "Manage items in the API")
    {
        // Add subcommands to this parent command
        this.AddCommand(new GetItemCommand());
        this.AddCommand(new CreateItemCommand());
    }
}
public class GetItemCommand : Command
{
    public GetItemCommand() : base("get", "Retrieve an item by ID or all items")
    {
        // Define an optional --id option to fetch a specific item
        var idOption = new Option<int?>(
            aliases: new[] { "--id", "-i" },
            description: "ID of the item to retrieve (omit to get all items)");
        this.AddOption(idOption);

        // (The global --json option is automatically available from the root command)
    }

    // The handler class that executes when this command is invoked
    public new class Handler : ICommandHandler
    {
        private readonly IApiClientService _apiClient;
        private readonly ILogger<Handler> _logger;

        // These properties will be bound to the command options automatically
        public int? Id { get; set; }         // Bound from --id
        public bool Json { get; set; }       // Bound from --json (global option)

        public Handler(IApiClientService apiClient, ILogger<Handler> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try 
            {
                // Call the API via the injected service
                if (Id.HasValue)
                {
                    var item = await _apiClient.GetItemAsync(Id.Value);
                    _logger.LogInformation("Retrieved item {Id} from API.", Id.Value);
                    Output(item);
                } 
                else 
                {
                    var items = await _apiClient.GetAllItemsAsync();
                    _logger.LogInformation("Retrieved {Count} items from API.", items.Count);
                    Output(items);
                }

                return 0; // success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from API");
                Console.Error.WriteLine($"Failed to retrieve data: {ex.Message}");
                return 1; // error code
            }
        }

        // Helper to output data in the requested format
        private void Output(object data)
        {
            if (Json)
            {
                // Serialize the data object to JSON
                string json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
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

public class CreateItemCommand : Command
    {
        public CreateItemCommand() : base("create", "Create a new item in the API")
        {
            var nameOption = new Option<string>(
                aliases: new[] { "--name", "-n" },
                description: "Name of the item") { IsRequired = true };
            var valueOption = new Option<int>(
                aliases: new[] { "--value", "-v" },
                description: "Value for the item") { IsRequired = true };

            this.AddOption(nameOption);
            this.AddOption(valueOption);
        }

        public new class Handler : ICommandHandler
        {
            private readonly IApiClientService _apiClient;
            private readonly ILogger<Handler> _logger;

            // These will be bound from CLI options
            public string? Name { get; set; }
            public int Value { get; set; }
            public bool Json { get; set; }

            public Handler(IApiClientService apiClient, ILogger<Handler> logger)
            {
                _apiClient = apiClient;
                _logger = logger;
            }

            public async Task<int> InvokeAsync(InvocationContext context)
            {
                try
                {
                    var newItem = new Item { Name = Name!, Value = Value };
                    var created = await _apiClient.CreateItemAsync(newItem);
                    _logger.LogInformation("Created item with ID {Id}.", created.Id);
                    if (Json)
                    {
                        string json = System.Text.Json.JsonSerializer.Serialize(created, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                        Console.WriteLine(json);
                    }
                    else
                    {
                        Console.WriteLine($"Created item #{created.Id}: {created.Name} = {created.Value}");
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating item in API");
                    Console.Error.WriteLine($"Failed to create item: {ex.Message}");
                    return 1;
                }
            }

            public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
        }
    }