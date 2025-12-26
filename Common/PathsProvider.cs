using System.Diagnostics;
using static Common.Constants;

namespace Common;

public static class PathsProvider
{
    public static string GetSourceRootDirectory()
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    public static string GetSolutionsProjectRootDirectory()
        => Path.Combine(GetSourceRootDirectory(), "Solutions");

    public static string GetCookieFilePath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var folder = Path.Combine(home, ".aoc");
        if (!Directory.Exists(folder)) 
            Directory.CreateDirectory(folder);
        return Path.Combine(folder, CookieFilename);
    }
    
    public static string GetSolutionTemplatePath()
        => Path.Combine(GetSourceRootDirectory(), EngineProjectName, "Resources", SolutionTemplateFileName);

    public static string GetInputFilePath((int year, int day) options)
    {
        var (year, day) = options;
        var dir = GetSolutionsProjectRootDirectory();
        return Path.Combine(dir, $"{year}/{day}/input.txt");
    }

    public static string GetInstructionsFilePath((int year, int day) options)
    {
        var (year, day) = options;
        return Path.Combine(GetSolutionsProjectRootDirectory(), $"{year}/{day}/{InstructionsFilename}");
    }

    public static string GetInstructionsTemplateFilePath()
        => Path.Combine(GetSourceRootDirectory(), EngineProjectName, "Resources", InstructionsTemplateName);

    public static string GetSolutionFilePath()
    {
        var stackTrace = new StackTrace(true);
        var stackFrame = stackTrace
            .GetFrames()
            .Where(f => !string.IsNullOrEmpty(f.GetFileName()))
            .Where(f => f.GetFileName()!.EndsWith(SolutionFileName))
            .FirstOrDefault() 
                       ?? throw new Exception($"StackFrame for {SolutionFileName} not found.");
        return Path.GetFullPath(stackFrame.GetFileName()!);
    }

    public static (string, string) GetYearAndDay()
    {
        return ParseYearAndDayFromSolutionFilePath(GetSolutionFilePath());

        static (string, string) ParseYearAndDayFromSolutionFilePath(string path)
        {
            var directoryName = Path.GetDirectoryName(path);
            var parts = directoryName!.Split(Path.DirectorySeparatorChar);
            var year = parts[^2];
            var day = parts[^1];
            return (year, day);
        }
    }
}