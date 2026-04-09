namespace Enjune.File;

public sealed class ExternalPath : ResourcePath
{
    private readonly string _absolutePath;
    
    public static ExternalPath Of(params string[] path) 
        => new(System.IO.Path.GetFullPath(System.IO.Path.Combine(path)));
    
    private ExternalPath(string absolutePath) => _absolutePath = absolutePath;

    public override ResourcePath Parent() => new ExternalPath(System.IO.Path.GetFullPath(System.IO.Path.Combine(_absolutePath, "..")));

    public override ResourcePath ThisDirectory() => new ExternalPath(System.IO.Path.GetFullPath(System.IO.Path.Combine(_absolutePath, ".")));

    public override ResourcePath Subdir(string subdir) => new ExternalPath(System.IO.Path.Combine(_absolutePath, subdir));

    protected override Stream? Read(out string? error)
    {
        try
        {
            var stream = System.IO.File.OpenRead(_absolutePath);
            error = null;
            return stream;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return null;
        }
    }

    public override string ToString() => _absolutePath;

    public override int GetHashCode() => _absolutePath.GetHashCode();
}