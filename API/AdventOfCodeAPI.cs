using Common;

namespace API;

public class Integration
{
    private HttpClient Client;

    public Integration(string cookie)
    {
        Client = new HttpClient();
        Client.DefaultRequestHeaders.Add("Cookie", cookie);
    }

    public async Task<string> GetInput((int year, int day) options) => await Get(options, true);
    public async Task<string> GetInstructions((int year, int day) options) => await Get(options);

    private async Task<string> Get((int year, int day) options, bool input = false)
    {
        var (year, day) = options;
        var url = $"https://adventofcode.com/{year}/day/{day}";
        if (input) url += "/input";

        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> SubmitAnswer(string answer, (int year, int day) options, Level level)
    {
        var (year, day) = options;
        var url = $"https://adventofcode.com/{year}/day/{day}/answer";

        var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                { "level", ((int)level).ToString() },
                { "answer", answer }
            });

        var response = await Client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}