using System.CommandLine;
using System.CommandLine.Invocation;
using static Common.PathsProvider;

namespace CLI;

public class ConfigCommand : Command
{
    public ConfigCommand() : base("config", "Configure settings")
    {
        var option = new Option<string>(
            ["--cookie", "-c"],
            "Advent of Code session cookie value");
        
        AddOption(option);
        
        this.SetHandler(Save, option);
    }

    private static async Task Save(string session)
    {
        if (string.IsNullOrWhiteSpace(session))
        {
            Console.WriteLine("Please provide a session cookie value.");
            return;
        }

        var path = GetCookieFilePath();
        await File.WriteAllTextAsync(path, session.Trim());
        Console.WriteLine($"Session cookie saved to: {path}");
    }
}
