using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Engine;
using Engine.Integrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Common.PathsProvider;

namespace CLI;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var description = "Advent of Code CLI - A developer tool for solving Advent of Code puzzles.";
        var rootCommand = new RootCommand(description);

        // var jsonOption = new Option<bool>(
        //     aliases: new[] { "--json" },
        //     description: "Output in JSON format");

        // rootCommand.AddGlobalOption(jsonOption);
        rootCommand.AddCommand(new InputsCommand());
        rootCommand.AddCommand(new InstructionsCommand());
        rootCommand.AddCommand(new ConfigCommand());
        rootCommand.AddCommand(new StartCommand());

        var builder = new CommandLineBuilder(rootCommand)
            .UseHost(_ => Host.CreateDefaultBuilder(args), hostBuilder =>
            {
                hostBuilder.ConfigureServices((_, services) =>
                {
                    // services.AddHttpClient();
                    services.AddTransient<IPuzzleEngine, PuzzleEngine>();
                    services.AddTransient<AdventOfCodeAPI>(provider =>
                    {
                        var path = GetCookieFilePath();
                        if (!File.Exists(path))
                            throw new Exception(
                                "Session cookie not found. Please run 'aoc config --session <value>' first.");
                        var cookie = File.ReadAllText(path);
                        return new AdventOfCodeAPI(cookie);
                    });
                    // services.AddTransient<IOutputFormatter, ConsoleOutputFormatter>();
                });

                hostBuilder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddFilter("Microsoft", LogLevel.Warning);
                    logging.AddFilter("System", LogLevel.Warning);
                    logging.SetMinimumLevel(LogLevel.Warning);
                });

                hostBuilder.UseCommandHandler<GetInputCommand, GetInputCommand.Handler>();
                hostBuilder.UseCommandHandler<GetInstructionCommand, GetInstructionCommand.Handler>();
                hostBuilder.UseCommandHandler<StartCommand, StartCommand.Handler>();
            })
            .UseDefaults()
            .Build();

        return await builder.InvokeAsync(args);
    }
}