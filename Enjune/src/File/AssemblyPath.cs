using System.Reflection;

namespace Enjune.File;

public sealed class AssemblyPath : Path
{
    private readonly string[] _path;
    private readonly Assembly _assembly;
    
    public AssemblyPath(Assembly assembly, params string[] path)
    {
        _assembly = assembly;
        _path = path;
    }

    public override Path Parent()
    {
       return new AssemblyPath(_assembly, _path.Take(_path.Length - 1).ToArray());
    }

    public override Path ThisDirectory()
    {
        _assembly.
    }

    public override Path Subdir(string subdir)
    {
        var newDirs =  new string[_path.Length + 1];
        _path.CopyTo(newDirs, 0);
        newDirs[_path.Length] = subdir;
        return new AssemblyPath(_assembly, newDirs);
    }

    private string GetSplitBy(char splitter) 
        => $"{_assembly.GetName().Name}{splitter}Resources{splitter}{string.Join(splitter, _path)}";

    public override string ToString() => GetSplitBy('.');

    protected override Stream? Read(out string? error)
    {
        var formedPath = GetSplitBy('.');
        error = null;
        Stream? stream;
        try
        {
            stream = _assembly.GetManifestResourceStream(formedPath);
        }
        catch (Exception exception)
        {
            error =  exception.Message;
            return null;
        }

        if (stream != null) return stream;
        error = $"stream is null: {formedPath}";
        return null;
    }

    public override void Write(out string? error, StreamReader data)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode() => GetSplitBy('.').GetHashCode();
}