namespace Enjune.File;

public class ResourcePath(params string[] path)
{
    public readonly string[] Path = path;
    
    public static implicit operator ResourcePath(string[] path) => new(path);
    public static implicit operator ResourcePath(string name) => new(name);

    public override string ToString()
    {
        return "Resources/" + string.Join('/',  Path);
    }
}