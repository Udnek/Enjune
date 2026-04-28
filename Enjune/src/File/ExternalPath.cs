namespace Enjune.File;

public sealed class ExternalPath : ResourcePath
{
    private readonly string _absolutePath;
    
    public static ExternalPath Of(params string[] path) 
        => new(Path.GetFullPath(Path.Combine(path)));
    
    private ExternalPath(string absolutePath) => _absolutePath = absolutePath;

    public override ExternalPath Parent() => new(Path.GetFullPath(Path.Combine(_absolutePath, "..")));

    public override ExternalPath ThisDirectory() => new(Path.GetFullPath(Path.Combine(_absolutePath, ".")));

    public override ExternalPath Subdir(string subdir) => new(Path.Combine(_absolutePath, subdir));
    
    protected override Stream? Read(out Error? error)
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