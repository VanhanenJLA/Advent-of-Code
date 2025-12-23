namespace Common;

public static class Utilities
{
    public static string LoadInput((int year, int day) options, string path)
        => File.ReadAllText(path);
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