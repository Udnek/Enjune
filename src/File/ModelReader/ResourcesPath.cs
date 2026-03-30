namespace Enjune.ModelReader;

public class ResourcesPath(params string[] path)
{
    public readonly string[] Path = path;
    
}

public static class S{
    extension(string[] path)
    {
        private ResourcesPath ToPath()
        {
            return new ResourcesPath(path);
        }
    }
}