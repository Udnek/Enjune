using System.Reflection;
using Enjune.Misc;

namespace Enjune.File;

public sealed class AssemblyPath : ResourcePath
{
    private readonly string[] _path;
    private readonly Assembly _assembly;
    
    public static AssemblyPath Of(Assembly assembly, params string[] path) => new(assembly, path);
    
    private AssemblyPath(Assembly assembly, params string[] path)
    {
        if (path.Length == 0 || path.Any(v => v.Length == 0)) 
            Logger.Error(this,$"trying to create path with incorrect parameter: {path.ContentToString()}");
        
        _assembly = assembly;
        _path = path;
    }

    public override ResourcePath Parent()
    {
        if (_path.Length == 0) return this;
        return Of(_assembly, _path.Take(_path.Length - 1).ToArray());
    }

    public override ResourcePath ThisDirectory()
    {
        return IsFile() ? Parent() : this;
    }

    public override ResourcePath Subdir(string subdir)
    {
        var workingDir = this;
        if (IsFile()) workingDir = (AssemblyPath) ThisDirectory();
        
        var newDirs = new string[workingDir._path.Length + 1];
        workingDir._path.CopyTo(newDirs, 0);
        newDirs[workingDir._path.Length] = subdir;
        return Of(_assembly, newDirs);
    }

    public bool IsFile() => _path.Length != 0 && _path[^1].Contains('.');

    private string GetSplitBy(char splitter)
    {
        if (_path.Length == 0) 
            return $"{_assembly.GetName().Name}{splitter}Resources";
        return $"{_assembly.GetName().Name}{splitter}Resources{splitter}{string.Join(splitter, _path)}";
    }

    protected override Stream? Read(out Error? error)
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
    
    public override string ToString() => GetSplitBy('.');

    public override int GetHashCode() => GetSplitBy('.').GetHashCode();
}