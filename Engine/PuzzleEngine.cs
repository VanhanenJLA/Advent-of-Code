using Common;
using HtmlAgilityPack;
using Engine.Integrations;
using Microsoft.Extensions.Logging;

namespace Engine;

public interface IPuzzleEngine
{
    Task<string> GetInput((int year, int day) options, bool forceRefresh = false);
    Task<string> GetInstructions((int year, int day) options, bool forceRefresh = false);
    Task<bool> SubmitAnswer(string answer, (int year, int day) options, Level level = Level.PartOne);
    Task<bool> Start((int year, int day) options);
    Task<bool> Unstart((int year, int day) options);
    HtmlNodeCollection ParseInstructions(string content);
}

public class PuzzleEngine : IPuzzleEngine
{
    private readonly ILogger<PuzzleEngine> _logger;
    private static AdventOfCodeAPI api => new(File.ReadAllText(GetCookieFilePath()));

    public PuzzleEngine(ILogger<PuzzleEngine> logger)
    {
        _logger = logger;
    }

    public HtmlNodeCollection ParseInstructions(string content)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        var nodes = doc.DocumentNode.SelectNodes("//article[@class='day-desc']");
        if (nodes is null || !nodes.Any())
            throw new Exception("Article node not found.");
        return nodes;
    }
    
    public async Task<bool> Start((int year, int day) options)
    {
        var (year, day) = options;
        var success = await CreateSolution((year, day));
        if (!success) 
            throw new Exception("Solution creation failed.");
        await GetInput((year, day));
        await GetInstructions((year, day));
        return true;
    }

    public async Task<bool> Unstart((int year, int day) options)
    {
        var (year, day) = options;
        var dir = Path.Combine(GetSolutionsProjectRootDirectory(), year.ToString(), day.ToString());
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
            _logger.LogInformation("Removed solution directory for {Year}/{Day}", year, day);
            return await Task.FromResult(true);
        }
        
        _logger.LogWarning("Solution directory for {Year}/{Day} not found at {Path}", year, day, dir);
        return await Task.FromResult(false);
    }

    public async Task<string> GetInput((int year, int day) options, bool forceRefresh = false)
    {
        var path = GetInputFilePath(options);
        if (File.Exists(path))
        {
            _logger.LogInformation("Retrieved input for {Year}/{Day} from local cache", options.year, options.day);
            return await File.ReadAllTextAsync(path);
        }

        _logger.LogInformation("Fetching input for {Year}/{Day} from remote", options.year, options.day);
        var input = await api.GetInput(options);
        await SaveInput(input, options);
        return input;
    }

    public async Task<string> GetInstructions((int year, int day) options, bool forceRefresh = false)
    {
        var path = GetInstructionsFilePath(options);
        
        if (!forceRefresh && File.Exists(path))
        {
            _logger.LogInformation("Retrieved instructions for {Year}/{Day} from local cache", options.year, options.day);
            return await File.ReadAllTextAsync(path);
        }

        _logger.LogInformation("Fetching instructions for {Year}/{Day} from remote", options.year, options.day);
        var content = await api.GetInstructions(options);
        var instructions = ParseInstructions(content);
        await SaveInstructions(options, instructions);
        
        return await File.ReadAllTextAsync(path);
    }

    public async Task<bool> SubmitAnswer(string answer, (int year, int day) options, Level level = Level.PartOne)
    {
        var (year, day) = options;
        var response = await api.SubmitAnswer(answer, (year, day), level);

        if (response.Contains(IncorrectAnswerResponse))
            return false;
        
        if (response.Contains(AlreadySolvedResponse))
            return true;

        if (response.Contains(CorrectAnswerResponse))
        {
            if (level != Level.PartOne) 
                return true;
            
            // We're advanced to part two so refresh instructions for Part Two.
            var instructionsPath = GetInstructionsFilePath(options);
            if (File.Exists(instructionsPath))
            {
                File.Delete(instructionsPath);
            }

            var instructions = await GetInstructions(options);
            return true;
        }
            

        throw new Exception($"Unmapped response: {response}");
    }

    private static async Task<bool> CreateSolution((int year, int day) options)
    {
        var (year, day) = options;
        var content = (await File
                .ReadAllTextAsync(GetSolutionTemplatePath()))
            .Replace(YearToken, year.ToString())
            .Replace(DayToken, day.ToString());
        var dir = Path.Combine(GetSolutionsProjectRootDirectory(), year.ToString(), day.ToString());

        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, SolutionFileName);
        if (File.Exists(file))
            throw new Exception($"Solution file already exists: {file}");
        await File.WriteAllTextAsync(file, content);
        return true;
    }
    
    private static async Task<bool> SaveInstructions((int year, int day) options, HtmlNodeCollection articles)
    {
        var template = new HtmlDocument();
        template.Load(GetInstructionsTemplateFilePath());

        var body =template
            .DocumentNode
            .SelectSingleNode("//body");

        foreach (var a in articles)
        {
            body.AppendChild(a);
        }
        var path = GetInstructionsFilePath(options);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) 
            Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(path, template.DocumentNode.OuterHtml);
        return true;
    }

    private static async Task<bool> SaveInput(string input, (int year, int day) options)
    {
        var path = GetInputFilePath(options);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) 
            Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(path, input.ReplaceLineEndings());
        return true;
    }
}