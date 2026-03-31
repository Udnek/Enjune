using System.Reflection;
using StbImageSharp;

namespace Enjune.File;

public static class FileManager
{
    
    public static ImageResult LoadAtlas() => LoadImage("atlas.png");

    public static string LoadText(ResourcePath path)
    {
        return LoadResource(path, s =>
        {
            using var streamReader = new StreamReader(s);
            return streamReader.ReadToEnd();
        });
    }
    
    public static ImageResult LoadImage(ResourcePath path)
    {
        return LoadResource(path, s => ImageResult.FromStream(s));
    }
    
    public static T LoadResource<T>(ResourcePath path, Func<Stream, T> streamTaker)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Enjune.Resources.{string.Join('.', path.Path)}");
        if (stream == null) throw new Exception("embedded resource not found");
        var result = streamTaker(stream);
        return result;
    }
}