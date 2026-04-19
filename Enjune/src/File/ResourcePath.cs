using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using StbImageSharp;

namespace Enjune.File;

public abstract class ResourcePath
{
    // interface

    protected abstract Stream? Read(out Error? error);

    public abstract ResourcePath Parent();
    public abstract ResourcePath ThisDirectory();
    public abstract ResourcePath Subdir(string subdir);

    public abstract override string ToString();

    public ResourcePath ResolveRaw(string raw)
    {
        var split = raw.Replace(@"\\", "/").Replace(@"\", "/").Split('/', StringSplitOptions.RemoveEmptyEntries);
        ResourcePath newResourcePath = this;
        foreach (var part in split)
        {
            if (part == ".") newResourcePath = newResourcePath.ThisDirectory();
            else if (part == "..") newResourcePath = newResourcePath.Parent();
            else newResourcePath = newResourcePath.Subdir(part);
        }
        return newResourcePath;
    }

    public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();
    
    public abstract override int GetHashCode();

    // misc

    public void LoadStream(out Error? error, Consumer<Stream> streamConsumer)
    {
        using var stream = Read(out error);
        if (stream == null) return;
        streamConsumer(stream);
    }
    
    public string? LoadText(out Error? error)
    {
        using var stream = Read(out error);
        if (stream == null) return null;
        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }
    
    public ByteImage? LoadImage(out Error? error)
    {
        using var stream = Read(out error);
        if (stream == null) return null;
        //StbImage.stbi_set_flip_vertically_on_load(1);
        var imageResult = ImageResult.FromStream(stream);
        ByteImage.ImType? type = imageResult.Comp switch
        {
            ColorComponents.RedGreenBlueAlpha => ByteImage.ImType.Rgba32,
            ColorComponents.RedGreenBlue => ByteImage.ImType.Rgb24,
            ColorComponents.Grey => ByteImage.ImType.Alpha8,
            _ => null
        };
        if (type == null)
        {
            error = $"unknown color components: {imageResult.Comp}";
            return null;
        }
        return new ByteImage(imageResult.Width, imageResult.Height, (ByteImage.ImType)type, imageResult.Data);
    }

    public byte[]? LoadBytes(out Error? error)
    {
        using var stream = Read(out error);
        if (stream == null) return null;
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}