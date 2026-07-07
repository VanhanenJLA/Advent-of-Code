using Engine;

namespace Engine.Tests;

public class RemoteEventStatusParserTests
{
    [Fact]
    public void Should_parse_event_star_counts_and_total()
    {
        const string html = """
                            <main>
                              <article>
                                <div class="eventlist-event"><a href="/">[2025]</a>          </div>
                                <div class="eventlist-event"><a href="/2024">[2024]</a>          </div>
                                <div class="eventlist-event"><a href="/2023">[2023]</a> <span class="star-count"> 9*</span> <span class="quiet">/ 50*</span></div>
                                <div class="eventlist-event"><a href="/2022">[2022]</a> <span class="star-count"> 6*</span> <span class="quiet">/ 50*</span></div>
                                <p>Total stars: <span class="star-count">15*</span></p>
                              </article>
                            </main>
                            """;

        var status = RemoteEventStatusParser.ParseEvents(html);

        Assert.Equal(15, status.TotalStars);
        Assert.Equal(200, status.TotalAvailableStars);
        Assert.Collection(
            status.Events,
            item =>
            {
                Assert.Equal(2025, item.Year);
                Assert.Equal(0, item.Stars);
                Assert.Equal(50, item.AvailableStars);
            },
            item =>
            {
                Assert.Equal(2024, item.Year);
                Assert.Equal(0, item.Stars);
                Assert.Equal(50, item.AvailableStars);
            },
            item =>
            {
                Assert.Equal(2023, item.Year);
                Assert.Equal(9, item.Stars);
                Assert.Equal(50, item.AvailableStars);
            },
            item =>
            {
                Assert.Equal(2022, item.Year);
                Assert.Equal(6, item.Stars);
                Assert.Equal(50, item.AvailableStars);
            });
    }
}
