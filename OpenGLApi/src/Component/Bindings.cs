using Enjune.Misc;

namespace OpenGLApi.Component;

public static class Bindings
{
    public static readonly int MaterialSsbo = TakeSsbo("materials");
    public static readonly int PerPrimitiveSsbo = TakeSsbo("perPrimitive");
 
    private static readonly List<string> TakenSsbo = [];
    private static readonly List<string> TakenUbo = [];

    public static int TakeSsbo(string name) => Take("ssbo", TakenSsbo, name);
    public static int TakeUbo(string name) => Take("ubo", TakenUbo, name);
    
    // name for debug purpose
    private static int Take(string typeName, List<string> taken, string name)
    {
        if (taken.Contains(name))
        {
            Logger.Error(typeof(Bindings), $"Binding among {typeName} for '{name}' was already taken, must be a bug");
            return -1;
        }
        taken.Add(name);
        
        Logger.Info(typeof(Bindings), $"Current bindings for {typeName}:");
        for (var i = 0; i < taken.Count; i++) 
            Logger.Info(typeof(Bindings), $"{i} -> {taken[i]}");

        return taken.Count - 1;
    }
    
}