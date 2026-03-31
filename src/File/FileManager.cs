using System.Reflection;
using StbImageSharp;

namespace Enjune.File;

public static class FileManager
{
    
    public static ImageResult LoadAtlas()
    {
        return LoadImage("atlas.png", out var error) ?? throw new Exception(error);
    }

    public static string? LoadText(ResourcePath path, out string? error)
    {
        return LoadResource(path,  out error, s =>
        {
            using var streamReader = new StreamReader(s);
            return streamReader.ReadToEnd();
        });
    }
    
    public static ImageResult? LoadImage(ResourcePath path, out string? error)
    {
        return LoadResource(path, out error, s => ImageResult.FromStream(s));
    }
    
    public static T? LoadResource<T>(ResourcePath path, out string? error, Func<Stream, T> streamTaker)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Enjune.Resources.{string.Join('.', path.Path)}");
        if (stream == null)
        {
            error = $"file with path \"{path}\" not found";
            return default;
        }
        error = null;
        var result = streamTaker(stream);
        return result;
    }
}