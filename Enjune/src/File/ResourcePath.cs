using Enjune.Misc;
using StbImageSharp;

namespace Enjune.File;

public abstract class ResourcePath
{
    // interface

    protected abstract Stream? Read(out string? error);

    public abstract void Write(out string? error, StreamReader data);

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
    
    public string? LoadText(out string? error)
    {
        return LoadResource(out error, s =>
        {
            using var streamReader = new StreamReader(s);
            return streamReader.ReadToEnd();
        });
    }
    
    public ImageResult? LoadImage(out string? error)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        return LoadResource(out error, s => ImageResult.FromStream(s, ColorComponents.RedGreenBlueAlpha));
    }

    public byte[]? LoadBytes(out string? error)
    {
        return LoadResource(out error, stream =>
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        });
    }
    
    public T? LoadResource<T>(out string? error, Func<Stream, T> streamTaker)
    {
        using var stream = Read(out error);
        if (stream == null)
        {
            error = $"stream is null: {error}";
            return default;
        }
        error = null;
        return streamTaker(stream);
    }
}