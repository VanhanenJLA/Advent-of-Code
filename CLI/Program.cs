using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using API;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static Common.Utilities;

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

        var builder = new CommandLineBuilder(rootCommand)
            .UseHost(_ => Host.CreateDefaultBuilder(args), hostBuilder =>
            {
                hostBuilder.ConfigureServices((_, services) =>
                {
                    // services.AddHttpClient();
                    services.AddTransient<AdventOfCodeAPI>(provider =>
                    {
                        var cookie = File.ReadAllText(GetCookieFilePath());
                        return new AdventOfCodeAPI(cookie);
                    });
                    // services.AddTransient<IOutputFormatter, ConsoleOutputFormatter>();
                });
                hostBuilder.UseCommandHandler<GetInputCommand, GetInputCommand.Handler>();
                hostBuilder.UseCommandHandler<GetInstructionCommand, GetInstructionCommand.Handler>();
            })
            .UseDefaults()
            .Build();

        return await builder.InvokeAsync(args);
    }
}