using Engine;

namespace Engine.Tests;

public class RemotePuzzleStatusParserTests
{
    [Fact]
    public void Should_parse_star_counts_from_calendar_classes()
    {
        const string html = """
                            <main>
                              <pre class="calendar">
                                <a href="/2023/day/1" class="calendar-verycomplete"><span class="calendar-day">1</span></a>
                                <a href="/2023/day/2" class="calendar-complete"><span class="calendar-day">2</span></a>
                                <a href="/2023/day/3"><span class="calendar-day">3</span></a>
                                <a href="/2022/day/1" class="calendar-verycomplete"><span class="calendar-day">1</span></a>
                              </pre>
                            </main>
                            """;

        var statuses = RemotePuzzleStatusParser.ParseCalendar(html, 2023);

        Assert.Collection(
            statuses,
            status =>
            {
                Assert.Equal(1, status.Day);
                Assert.Equal(2, status.Stars);
            },
            status =>
            {
                Assert.Equal(2, status.Day);
                Assert.Equal(1, status.Stars);
            },
            status =>
            {
                Assert.Equal(3, status.Day);
                Assert.Equal(0, status.Stars);
            });
    }
}
