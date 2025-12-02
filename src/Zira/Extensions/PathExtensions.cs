namespace Zira.Extensions;

public static class PathExtensions
{
    public static string CombinePath(this string path, params string[] parts)
    {
        var paths = new List<string> { path };
        paths.AddRange(parts);
        return Path.Combine([.. paths]);
    }
}
