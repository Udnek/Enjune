using System.Reflection;
using StbImageSharp;

namespace Enjune;

public static class FileManager
{
    public static ImageResult LoadAtlas()
    {
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enjune.Resources.atlas.png"))
        {
            if (stream == null) throw new Exception("embedded resource not found");
            var imageResult = ImageResult.FromStream(stream);
            return imageResult;
        }
    }

}