using System.Reflection;
using StbImageSharp;

namespace Enjune.File;

// public static class FileManager
// {
//     public static string? LoadText(ResourcePath path, out Error? error)
//     {
//         return LoadResource(path,  out error, s =>
//         {
//             using var streamReader = new StreamReader(s);
//             return streamReader.ReadToEnd();
//         });
//     }
//     
//     public static ImageResult? LoadImage(ResourcePath path, out Error? error)
//     {
//         StbImage.stbi_set_flip_vertically_on_load(1);
//         var image = LoadResource(path, out error, s => ImageResult.FromStream(s, ColorComponents.RedGreenBlueAlpha));
//         return image;
//     }
//     
//     public static T? LoadResource<T>(ResourcePath path, out Error? error, Func<Stream, T> streamTaker)
//     {
//         using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Enjune.{path.GetSplitBy(".")}");
//         if (stream == null)
//         {
//             error = $"file with path \"{path}\" not found";
//             return default;
//         }
//         error = null;
//         var result = streamTaker(stream);
//         return result;
//     }
// }