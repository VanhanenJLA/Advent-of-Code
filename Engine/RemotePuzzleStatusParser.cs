using System.Text.RegularExpressions;
using Common;
using HtmlAgilityPack;
using static Common.Constants;

namespace Engine;

public record RemotePuzzleStatus(int Day, int Stars);

public static class RemotePuzzleStatusParser
{
    private static readonly Regex DayHrefPattern = new(@"/(?<year>\d{4})/day/(?<day>\d{1,2})", RegexOptions.Compiled);

    public static IReadOnlyList<RemotePuzzleStatus> ParseCalendar(string html, int year)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var links = document.DocumentNode.SelectNodes($"//a[contains(@href, '/{year}/day/')]");
        if (links is null)
            return Array.Empty<RemotePuzzleStatus>();

        return links
            .Select(link => ParseDay(link, year))
            .Where(status => status is not null)
            .Select(status => status!)
            .GroupBy(status => status.Day)
            .Select(group => new RemotePuzzleStatus(group.Key, group.Max(status => status.Stars)))
            .OrderBy(status => status.Day)
            .ToArray();
    }

    private static RemotePuzzleStatus? ParseDay(HtmlNode link, int year)
    {
        var href = link.GetAttributeValue("href", string.Empty);
        var match = DayHrefPattern.Match(href);
        if (!match.Success)
            return null;

        var parsedYear = int.Parse(match.Groups["year"].Value);
        if (parsedYear != year)
            return null;

        var day = int.Parse(match.Groups["day"].Value);
        return new RemotePuzzleStatus(day, CountStars(link));
    }

    private static int CountStars(HtmlNode link)
    {
        if (HasClass(link, CalendarVeryCompleteClass))
            return 2;

        if (HasClass(link, CalendarCompleteClass))
            return 1;

        return 0;
    }

    private static bool HasClass(HtmlNode node, string className)
    {
        var classes = node.GetAttributeValue("class", string.Empty);
        return classes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(className, StringComparer.Ordinal);
    }
}
