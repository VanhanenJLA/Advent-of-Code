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
        // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
        var solutionFrame = stackTrace
            .GetFrames()
            .Select(frame => frame.GetFileName())
            .FirstOrDefault(filename => !string.IsNullOrEmpty(filename)
                                        && filename.EndsWith("Solution.cs"));
        return Path.GetFullPath(solutionFrame ?? throw new InvalidOperationException());
    }

    public static string GetSourceRootDirectory()
        => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..");
    // TODO: Enumerable.Repeat("..", 5)

    public static string GetSolutionsProjectRootDirectory()
        => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Solutions");

    public static string GetCookieFilePath()
        => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Client", CookieFilename);

    public static string GetInputClassTemplatePath()
        => Path.Combine(GetSourceRootDirectory(), "Client", InputClassTemplateName);

    public static string LoadInput((int year, int day, InputFile.Type type) options)
        => File.ReadAllText(GetInputFilePath(options));

    public static string GetInputFilePath((int year, int day, InputFile.Type type) options)
    {
        var (year, day, type) = options;
        var dir = GetSolutionsProjectRootDirectory();
        var filename = InputFile.GetFilename(type);
        return Path.Combine(dir, $"{year}/{day}/{filename}");
    }

    public static string GetInstructionsFilePath((int year, int day) options)
    {
        var (year, day) = options;
        return Path.Combine(GetSolutionsProjectRootDirectory(), $"{year}/{day}/{InstructionsFilename}");
    }

    public static string GetInstructionsTemplateFilePath()
        => Path.Combine(GetSolutionsProjectRootDirectory(), InstructionsTemplateName);
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
        _ => throw new ArgumentException("Unknown input file type: " + t)
    };
}