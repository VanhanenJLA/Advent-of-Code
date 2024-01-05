using System.Diagnostics;
using HtmlAgilityPack;

namespace Advent_of_Code;

public static class Utilities
{
    public static (string, string) GetYearAndDay()
    {
        return ParseYearAndDayFromSolutionFilePath(GetSolutionFilePath());
        
        static (string, string) ParseYearAndDayFromSolutionFilePath(string path)
        {
            var directoryName = Path.GetDirectoryName(path);
            var parts = directoryName.Split(Path.DirectorySeparatorChar);
            var year = parts[^2];
            var day = parts[^1];
            return (year, day);
        }

        static string GetSolutionFilePath()
        {
            var stackTrace = new StackTrace(true);
            // ReSharper disable ReplaceWithSingleCallToFirstOrDefault
            var solutionFrame = stackTrace
                .GetFrames()
                .Select(frame => frame.GetFileName())
                .Where(filename => !string.IsNullOrEmpty(filename))
                .Where(filename => filename.EndsWith("Solution.cs"))
                .FirstOrDefault();

            if (solutionFrame == null)
                throw new Exception("Solution.cs file path not found.");
            return Path.GetFullPath(solutionFrame);
        }
    }

    public static string GetSourceRootDirectory()
        => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..");
    // TODO: Enumerable.Repeat("..", 5)
    
    public static string GetSolutionsProjectRootDirectory()
        => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Solutions");
    
    public static void ParseAndSaveInstructions(string content, (int year, int day) options)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);
        var articleNode = doc.DocumentNode.SelectSingleNode("//article[@class='day-desc']");

        var template = new HtmlDocument();
        template.Load(GetInstructionsTemplateFilePath());

        template
            .DocumentNode
            .SelectSingleNode("//body")
            .AppendChild(articleNode);

        File.WriteAllText(GetInstructionsFilePath(options), template.DocumentNode.OuterHtml);
    }

    public static void SaveInput(string input, (int year, int day) options) 
        => File.WriteAllText(GetInputFilePath(options), input);

    public static string GetInputFilePath((int year, int day) options)
    {
        var (year, day) = options;
        var dir = GetSolutionsProjectRootDirectory();
        return Path.Combine(dir, $"{year}/{day}/input.txt");
    }
    
    public static string GetInstructionsFilePath((int year, int day) options)
    {
        var (year, day) = options;
        return Path.Combine(GetSolutionsProjectRootDirectory(), $"{year}/{day}/input.txt");
    }   
    
    public static string GetInstructionsTemplateFilePath() 
        => Path.Combine(GetSolutionsProjectRootDirectory(), "instructions_template.html");
}

public class SolutionBase
{
    readonly string year;
    readonly string day;

    protected SolutionBase()
    {
        var (year, day) = Utilities.GetYearAndDay();
        this.year = year;
        this.day = day;
    }

    public void Deconstruct(out int year, out int day)
    {
        year = Convert.ToInt32(this.year);
        day = Convert.ToInt32(this.day);
    }
}

public interface ISolution
{
    string SolvePartOne(string input);
    string SolvePartTwo(string input);
    
    void Deconstruct(out int year, out int day);
    
}

