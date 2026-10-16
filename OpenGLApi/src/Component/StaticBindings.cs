using Enjune.Misc;

namespace OpenGLApi.Component;

public static class Bindings
{
    private static readonly Dictionary<string, int> TakenSsbo = [];
    private static readonly Dictionary<string, int> TakenUbo = [];
    
    public static readonly int MaterialSsbo = TakeSsbo("materials");
    public static readonly int PerPrimitiveSsbo = TakeSsbo("perPrimitive");

    public static int TakeSsbo(string name) => Take("ssbo", TakenSsbo, name);
    public static int TakeUbo(string name) => Take("ubo", TakenUbo, name);
    
    // name for debug purpose
    private static int Take(string typeName, Dictionary<string, int> taken, string name)
    {
        if (taken.TryGetValue(name, out var binding))
            return binding;

        taken[name] = taken.Count;
        Logger.Info(typeof(Bindings), $"Current bindings for {typeName}:");
        foreach (var (key, bind) in taken) 
            Logger.Info(typeof(Bindings), $"{key} -> {bind}");

        return taken.Count - 1;
    }
    
}