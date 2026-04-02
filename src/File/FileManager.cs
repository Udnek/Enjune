using System.Reflection;
using StbImageSharp;

namespace Enjune.File;

public static class FileManager
{
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
        return LoadResource(path, out error, s => ImageResult.FromStream(s, ColorComponents.RedGreenBlueAlpha));
    }
    
    public static T? LoadResource<T>(ResourcePath path, out string? error, Func<Stream, T> streamTaker)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Enjune.{path.GetSplitBy(".")}");
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