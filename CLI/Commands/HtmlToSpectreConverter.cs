using System.Text.RegularExpressions;
using Spectre.Console;

namespace CLI;

public static partial class HtmlToSpectreConverter
{
    public static void RenderHtmlToConsole(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return;

        var lastIndex = 0;

        foreach (Match match in PreBlock().Matches(html))
        {
            RenderMarkupFragment(html[lastIndex..match.Index]);
            RenderPreBlock(match.Groups[1].Value);
            lastIndex = match.Index + match.Length;
        }

        RenderMarkupFragment(html[lastIndex..]);
    }

    public static string ConvertHtmlToSpectreMarkup(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;
        
        html = Article().Replace(html, "");
        html = Heading2().Replace(html, "[bold]$1[/]\n\n");
        html = Paragraph().Replace(html, "$1\n");
        html = EmStar().Replace(html, "[bold yellow]$1[/]");
        html = Emphasis().Replace(html, "[bold]$1[/]");
        html = Anchor().Replace(html, "[link=$1][green]$2[/][/]");
        html = Code().Replace(html, "[white on grey]$1[/]");
        html = Pre().Replace(html, "[white on grey]$1[/]\n");
        html = UL().Replace(html, "");
        html = LI().Replace(html, "- $1");
        html = Unmatched().Replace(html, "");
        return html;
    }

    private static void RenderMarkupFragment(string html)
    {
        var markup = ConvertHtmlToSpectreMarkup(html);
        if (!string.IsNullOrWhiteSpace(markup))
            AnsiConsole.Markup(markup);
    }

    private static void RenderPreBlock(string html)
    {
        var content = ConvertPreformattedHtmlToSpectreMarkup(html).TrimEnd('\r', '\n');
        if (string.IsNullOrWhiteSpace(content))
            return;

        AnsiConsole.Write(new Panel(new Markup(content))
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Grey))
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();
    }

    private static string ConvertPreformattedHtmlToSpectreMarkup(string html)
    {
        html = CodeWrapper().Replace(html, "$1");
        html = EmStar().Replace(html, "[bold yellow]$1[/]");
        html = Emphasis().Replace(html, "[bold]$1[/]");
        html = Anchor().Replace(html, "[link=$1][green]$2[/][/]");
        html = Unmatched().Replace(html, "");
        return html;
    }

    [GeneratedRegex(@"<\/?article[^>]*>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Article();
    [GeneratedRegex(@"<h2\b[^>]*>(.*?)<\/h2>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Heading2();
    [GeneratedRegex(@"<p>(.*?)<\/p>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Paragraph();
    [GeneratedRegex(@"<em\s+class\s*=\s*[""']star[""']\s*>(.*?)<\/em>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex EmStar();
    [GeneratedRegex(@"<em(?!\s+class\s*=\s*[""']star[""'])>(.*?)<\/em>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Emphasis();
    [GeneratedRegex(@"<a\b[^>]*\bhref\s*=\s*[""']([^""']+)[""'][^>]*>(.*?)<\/a>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Anchor();
    [GeneratedRegex(@"<code>(.*?)<\/code>", RegexOptions.IgnoreCase | RegexOptions.Singleline, "fi-FI")]
    private static partial Regex CodeWrapper();
    [GeneratedRegex(@"<code>(.*?)<\/code>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex Code();
    [GeneratedRegex(@"<pre>(.*?)<\/pre>", RegexOptions.IgnoreCase | RegexOptions.Singleline, "fi-FI")]
    private static partial Regex Pre();
    [GeneratedRegex(@"<pre>(.*?)<\/pre>", RegexOptions.IgnoreCase | RegexOptions.Singleline, "fi-FI")]
    private static partial Regex PreBlock();
    [GeneratedRegex(@"<\/?ul>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex UL();
    [GeneratedRegex(@"<li>(.*?)<\/li>", RegexOptions.IgnoreCase, "fi-FI")]
    private static partial Regex LI();
    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex Unmatched();
}