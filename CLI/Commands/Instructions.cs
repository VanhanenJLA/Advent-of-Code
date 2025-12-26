using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        
        // (The global --json option is automatically available from the root command)
    }

    public new class Handler : ICommandHandler
    {
        private readonly IPuzzleEngine _puzzleEngine;
        private readonly ILogger<Handler> _logger;
        public int Day { get; set; }
        public int? Year { get; set; }
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
                var instructions = await _puzzleEngine.GetInstructions((Year.Value, Day));
                var articles = _puzzleEngine.ParseInstructions(instructions);
                foreach (var a in articles)
                {
                    var markup = HtmlToSpectreConverter.ConvertHtmlToSpectreMarkup(a.OuterHtml);
                    AnsiConsole.MarkupLine(markup);
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

public static partial class HtmlToSpectreConverter
{
    public static string ConvertHtmlToSpectreMarkup(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;
        
        html = Article().Replace(html, "");
        html = Heading2().Replace(html, "[bold]$1[/]\n\n");
        html = Paragraph().Replace(html, "$1\n");
        html = EmStar().Replace(html, "[bold yellow]$1[/]");
        html = Emphasis().Replace(html, "[bold]$1[/]");
        html = Code().Replace(html, "[white on grey]$1[/]");
        html = Pre().Replace(html, "[white on grey]$1[/]\n");
        html = UL().Replace(html, "");
        html = LI().Replace(html, "- $1");
        html = Unmatched().Replace(html, "");
        return html;
    }

    [GeneratedRegex(@"<\/?article[^>]*>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Article();
    [GeneratedRegex(@"<h2>(.*?)<\/h2>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Heading2();
    [GeneratedRegex(@"<p>(.*?)<\/p>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Paragraph();
    [GeneratedRegex(@"<em\s+class\s*=\s*[""']star[""']\s*>(.*?)<\/em>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex EmStar();
    [GeneratedRegex(@"<em(?!\s+class\s*=\s*[""']star[""'])>(.*?)<\/em>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Emphasis();
    [GeneratedRegex(@"<code>(.*?)<\/code>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Code();
    [GeneratedRegex(@"<pre>(.*?)<\/pre>", RegexOptions.IgnoreCase | RegexOptions.Singleline, "fi-FI")]
    private static partial Regex Pre();
    [GeneratedRegex(@"<\/?ul>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex UL();
    [GeneratedRegex(@"<li>(.*?)<\/li>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex LI();
    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex Unmatched();
}


