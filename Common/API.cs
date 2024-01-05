namespace Common;

public class API
{
    // TODO: Cookie enviin
    const string sessionCookie =
        "session=53616c7465645f5f8a575550166e5b698824a3cc45cf66183db6b9dc6de7d6e209d2cef4e9347314cdf192ad02999e6b4ab52bb8c64133641f02da3534187df8";

    static readonly HttpClient client = new() { DefaultRequestHeaders = { { "Cookie", sessionCookie } } };

    public static async Task<string> GetInput((int year, int day) options) => await Get(options, true);

    public static async Task<string> GetInstructions((int year, int day) options) => await Get(options);

    private static async Task<string> Get((int year, int day) options, bool input = false)
    {
        var (year, day) = options;
        var url = $"https://adventofcode.com/{year}/day/{day}";
        if (input) url += "/input";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<string> SubmitAnswer(string answer, Level level, (int year, int day) options)
    {
        var (year, day) = options;
        var url = $"https://adventofcode.com/{year}/day/{day}/answer";

        var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                { "level", level.ToString() },
                { "answer", answer }
            });

        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

public enum Level
{
    PuzzlePartOne = 1,
    PuzzlePartTwo = 2
}