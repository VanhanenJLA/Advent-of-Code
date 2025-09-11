using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CLI;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Define the root command with a description
        var rootCommand = new RootCommand("MyApiClient CLI - A tool to interact with the Example API");

        // (Optional) Define a global option for output format (e.g., JSON)
        var jsonOption = new Option<bool>(
            aliases: new[] { "--json" },
            description: "Output in JSON format");
        rootCommand.AddGlobalOption(jsonOption);

        // Add subcommands to the root (e.g., an 'items' command with further subcommands)
        rootCommand.AddCommand(new ItemsCommand());

        // Build the command-line parser with Hosting (for DI, logging, config)
        var builder = new CommandLineBuilder(rootCommand)
            .UseHost(_ => Host.CreateDefaultBuilder(args), hostBuilder =>
            {
                // Configure services (DI) and link commands to their handlers
                hostBuilder.ConfigureServices((hostContext, services) =>
                {
                    IConfiguration config = hostContext.Configuration;
                    // Register application services
                    services.AddHttpClient<IApiClientService, ApiClientService>(client =>
                    {
                        // Use base URL from configuration (appsettings.json or env var)
                        client.BaseAddress = new Uri(config["Api:BaseUrl"]);
                    });
                    services.AddTransient<IOutputFormatter, ConsoleOutputFormatter>(); // for formatted output, if using
                });
                // Map command classes to their handler classes for DI
                hostBuilder.UseCommandHandler<GetItemCommand, GetItemCommand.Handler>();
                hostBuilder.UseCommandHandler<CreateItemCommand, CreateItemCommand.Handler>();
            })
            .UseDefaults()
            .Build();

        // Invoke the parser to execute the appropriate command
        return await builder.InvokeAsync(args);
    }
}