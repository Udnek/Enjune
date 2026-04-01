namespace Enjune.File;

public class ResourcePath(params string[] path)
{
    private readonly string[] _path = path;
    
    public static implicit operator ResourcePath(string[] path) => new(path);
    public static implicit operator ResourcePath(string name) => new(name);

    public ResourcePath ResolveFromLocal(string slashedPath)
    {
        var newPath = new List<string>(_path);
        newPath.RemoveAt(newPath.Count - 1); // dir where current reosource
        if (slashedPath.StartsWith('.'))
        {
            slashedPath = slashedPath.Substring(2); // './'
        }
        var dirs = slashedPath.Replace(@"\", @"/").Split('/', StringSplitOptions.RemoveEmptyEntries);
        newPath.AddRange(dirs);
        return new ResourcePath(newPath.ToArray());
    }

    public string GetSplitBy(string splitter)
    {
        return "Resources" + splitter + string.Join(splitter, _path);
    }

    public override string ToString() => GetSplitBy("/");

    public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();

    public override int GetHashCode() => GetSplitBy("/").GetHashCode();
}