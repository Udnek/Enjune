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

    public override Path Parent() => new AssemblyPath(_assembly, _path.Take(_path.Length - 1).ToArray());

    public override Path ThisDirectory()
    {
        if (_path.Length == 0) return this;
        if (_path[^1].Contains('.')) return Parent();
        return this; // we are already dir
    }

    public override Path Subdir(string subdir)
    {
        var newDirs =  new string[_path.Length + 1];
        _path.CopyTo(newDirs, 0);
        newDirs[_path.Length] = subdir;
        return new AssemblyPath(_assembly, newDirs);
    }

    private string GetSplitBy(char splitter)
    {
        if (_path.Length == 0) 
            return $"{_assembly.GetName().Name}{splitter}Resources";
        return $"{_assembly.GetName().Name}{splitter}Resources{splitter}{string.Join(splitter, _path)}";
    }

    protected override Stream? Read(out string? error)
    {
        var formedPath = GetSplitBy('.');
        var stream = _assembly.GetManifestResourceStream(formedPath);
        if (stream == null)
        {
            error = $"embedded \"{formedPath}\" not found";
            return null;
        }
        error = null;
        return stream;
    }

    public override void Write(out string? error, StreamReader data)
    {
        throw new NotImplementedException();
    }
    
    public override string ToString() => GetSplitBy('.');

    public override int GetHashCode() => GetSplitBy('.').GetHashCode();
}