using System.Reflection;
using System.Text;
using Enjune.Misc;
using StbImageSharp;

namespace Enjune;

public static class FileManager
{
    public static ImageResult LoadAtlas() => LoadImage("atlas.png");

    public static string LoadText(params string[] path)
    {
        return LoadResource(path, s =>
        {
            using var streamReader = new StreamReader(s);
            return streamReader.ReadToEnd();
        });
    }
    
    public static ImageResult LoadImage(params string[] path)
    {
        return LoadResource(path, s => ImageResult.FromStream(s));
    }
    
    public static T LoadResource<T>(string[] path, Func<Stream, T> streamTaker)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Enjune.Resources.{string.Join('.', path)}");
        if (stream == null) throw new Exception("embedded resource not found");
        var result = streamTaker(stream);
        return result;
    }
}