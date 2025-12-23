using System.Diagnostics;
using static Common.Constants;

namespace Common;

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
    }

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
    public static string GetSourceRootDirectory()
        => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..");
    // TODO: Enumerable.Repeat("..", 5)

    public static string GetSolutionsProjectRootDirectory()
        => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Solutions");

    public static string GetCookieFilePath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var folder = Path.Combine(home, ".aoc");
        if (!Directory.Exists(folder)) 
            Directory.CreateDirectory(folder);
        return Path.Combine(folder, CookieFilename);
    }
    
    public static string GetSolutionTemplatePath()
        => Path.Combine(GetSourceRootDirectory(), DeveloperClientProjectName, SolutionTemplateFileName);

    public static string LoadInput((int year, int day) options)
        => File.ReadAllText(GetInputFilePath(options));

    public static string GetInputFilePath((int year, int day) options)
    {
        var (year, day) = options;
        var dir = GetSolutionsProjectRootDirectory();
        var filename = InputFile.GetFilename(InputFile.Type.Text);
        return Path.Combine(dir, $"{year}/{day}/{filename}");
    }

    public static string GetInstructionsFilePath((int year, int day) options)
    {
        var (year, day) = options;
        return Path.Combine(GetSolutionsProjectRootDirectory(), $"{year}/{day}/{InstructionsFilename}");
    }

    public static string GetInstructionsTemplateFilePath()
        => Path.Combine(GetSourceRootDirectory(), DeveloperClientProjectName, InstructionsTemplateName);
}

public static class InputFile
{
    public enum Type
    {
        Text,
    }

    public static string GetFilename(Type t) => t switch
    {
        Type.Text => "input.txt",
        _ => throw new ArgumentException($"Unknown input file type: {t}")
    };
}