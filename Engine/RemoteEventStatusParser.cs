using System.Text.RegularExpressions;
using Common;
using HtmlAgilityPack;
using static Common.Constants;

namespace Engine;

public record RemoteEventStatus(int Year, int Stars, int AvailableStars);

public record RemoteEventsStatus(IReadOnlyList<RemoteEventStatus> Events, int TotalStars, int TotalAvailableStars);

public static class RemoteEventStatusParser
{
    private static readonly Regex YearPattern = new(@"(?<year>\d{4})", RegexOptions.Compiled);
    private static readonly Regex StarsPattern = new(@"(?<stars>\d+)\*", RegexOptions.Compiled);
    private static readonly Regex AvailableStarsPattern = new(@"/\s*(?<stars>\d+)\*", RegexOptions.Compiled);

    public static RemoteEventsStatus ParseEvents(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var eventNodes = document.DocumentNode.SelectNodes(
            $"//*[contains(concat(' ', normalize-space(@class), ' '), ' {EventListEventClass} ')]");

        var events = eventNodes is null
            ? Array.Empty<RemoteEventStatus>()
            : eventNodes
                .Select(ParseEvent)
                .Where(status => status is not null)
                .Select(status => status!)
                .OrderByDescending(status => status.Year)
                .ToArray();

        return new RemoteEventsStatus(
            events,
            ParseTotalStars(document) ?? events.Sum(status => status.Stars),
            events.Sum(status => status.AvailableStars));
    }

    private static RemoteEventStatus? ParseEvent(HtmlNode eventNode)
    {
        var link = eventNode.SelectSingleNode(".//a");
        if (link is null)
            return null;

        var yearMatch = YearPattern.Match(HtmlEntity.DeEntitize(link.InnerText));
        if (!yearMatch.Success)
            return null;

        var year = int.Parse(yearMatch.Groups["year"].Value);
        var stars = ParseStars(eventNode.SelectSingleNode(
            $".//*[contains(concat(' ', normalize-space(@class), ' '), ' {StarCountClass} ')]"));
        var availableStars = ParseAvailableStars(eventNode);

        return new RemoteEventStatus(year, stars, availableStars);
    }

    private static int? ParseTotalStars(HtmlDocument document)
    {
        var totalNode = document.DocumentNode.SelectSingleNode(
            $"//p[contains(., 'Total stars:')]//*[contains(concat(' ', normalize-space(@class), ' '), ' {StarCountClass} ')]");

        return totalNode is null ? null : ParseStars(totalNode);
    }

    private static int ParseStars(HtmlNode? node)
    {
        if (node is null)
            return 0;

        var match = StarsPattern.Match(HtmlEntity.DeEntitize(node.InnerText));
        return match.Success
            ? int.Parse(match.Groups["stars"].Value)
            : 0;
    }

    private static int ParseAvailableStars(HtmlNode eventNode)
    {
        var text = HtmlEntity.DeEntitize(eventNode.InnerText);
        var match = AvailableStarsPattern.Match(text);
        return match.Success
            ? int.Parse(match.Groups["stars"].Value)
            : EventStarCapacity;
    }
}
