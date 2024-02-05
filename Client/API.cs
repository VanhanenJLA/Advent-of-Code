namespace Common;

public class API
{
    
    static readonly HttpClient client = new() { DefaultRequestHeaders = { { "Cookie", File.ReadAllText(GetCookieFilePath()) } } };

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

    public static async Task<string> SubmitAnswer(string answer, (int year, int day) options, Level level)
    {
        var (year, day) = options;
        var url = $"https://adventofcode.com/{year}/day/{day}/answer";

        var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                { "level", ((int) level).ToString() },
                { "answer", answer }
            });

        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}