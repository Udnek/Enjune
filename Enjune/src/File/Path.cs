using StbImageSharp;

namespace Enjune.File;

public abstract class Path
{
    // interface

    protected abstract Stream? Read(out string? error);

    public abstract void Write(out string? error, StreamReader data);

    public abstract Path Parent();
    public abstract Path ThisDirectory();
    public abstract Path Subdir(string subdir);

    public abstract override string ToString();

    public Path ResolveRaw(string raw)
    {
        var split = raw.Replace(@"\\", "/").Replace(@"\", "/").Split('/', StringSplitOptions.RemoveEmptyEntries);
        Path newPath = this;
        foreach (var part in split)
        {
            if (part == ".") newPath = newPath.ThisDirectory();
            else if (part == "..") newPath = newPath.Parent();
            else newPath = newPath.Subdir(part);
        }
        return newPath;
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
        var image = LoadResource(out error, s => ImageResult.FromStream(s, ColorComponents.RedGreenBlueAlpha));
        return image;
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