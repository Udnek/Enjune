namespace Enjune.File;

public class ResourcePath(params string[] path)
{
    public readonly string[] Path = path;
    
    public static implicit operator ResourcePath(string[] path) => new(path);
    public static implicit operator ResourcePath(string name) => new(name);

    public ResourcePath Resolve(string slashedPath)
    {
        var newPath = new List<string>(Path);
        if (slashedPath.StartsWith('.'))
        {
            slashedPath = slashedPath.Substring(1);
            newPath.RemoveAt(newPath.Count - 1);
        }
        var dirs = slashedPath.Replace(@"\", @"/").Split('/', StringSplitOptions.RemoveEmptyEntries);
        newPath.AddRange(dirs);
        return new ResourcePath(newPath.ToArray());
    }
    
    public override string ToString()
    {
        return "Resources/" + string.Join('/',  Path);
    }
}