namespace Enjune.File;

public sealed class ExternalPath : Path
{
    private readonly string _absolutePath;
    
    public ExternalPath(params string[] path)
    {
        _absolutePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(path));
    }


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

    public override void Write(out string? error, StreamReader data)
    {
        throw new NotImplementedException();
    }

    public override Path Parent()
    {
        throw new NotImplementedException();
    }

    public override Path ThisDirectory()
    {
        throw new NotImplementedException();
    }

    public override Path Subdir(string subdir)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode() => _absolutePath.GetHashCode();
}