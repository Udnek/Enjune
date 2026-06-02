using System.Reflection;
using Enjune.Data;
using Enjune.Misc;

namespace Enjune.File;

public sealed class AssemblyPath : ResourcePath
{
    public new static readonly Codec<AssemblyPath> Codec = Codecs
        .ForConstructor(args => Of((Assembly)args[0]!, (string[])args[1]!))
        .ForField("assembly", i => i.Assembly, Codecs.Assembly, Enjune.Assembly)
        .ForField("path", i => i._path, Codecs.String.Array, [])
        .Build();
    
    private readonly string[] _path;
    public readonly Assembly Assembly;
    
    public static AssemblyPath Of(Assembly assembly, params string[] path) => new(assembly, path);
    
    private AssemblyPath(Assembly assembly, params string[] path)
    {
        if (path.Length == 0 || path.Any(v => v.Length == 0)) 
            Logger.Error(this,$"trying to create path with incorrect parameter: {path.ContentToString()}");
        
        Assembly = assembly;
        _path = path;
    }

    public override ResourcePath Parent()
    {
        if (_path.Length == 0) return this;
        return Of(Assembly, _path.Take(_path.Length - 1).ToArray());
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
        return Of(Assembly, newDirs);
    }

    public bool IsFile() => _path.Length != 0 && _path[^1].Contains('.');

    private string GetSplitBy(char splitter)
    {
        if (_path.Length == 0) 
            return $"{Assembly.GetName().Name}{splitter}Resources";
        return $"{Assembly.GetName().Name}{splitter}Resources{splitter}{string.Join(splitter, _path)}";
    }

    protected override Stream? Read(out Error? error)
    {
        var formedPath = GetSplitBy('.');
        var stream = Assembly.GetManifestResourceStream(formedPath);
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