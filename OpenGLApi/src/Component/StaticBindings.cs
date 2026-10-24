using Enjune.Misc;

namespace OpenGLApi.Component;

public static class StaticBindings
{
    private static readonly Dictionary<string, int> TakenSsbo = [];
    private static readonly Dictionary<string, int> TakenUbo = [];
    private static readonly Dictionary<string, int> TakenTexUnits = [];

    public static readonly TextureUnit AllTextures = TextureUnit.Texture0; 
    
    public static readonly int PerPrimitiveSsbo = TakeSsbo("perPrimitive");

    public static int TakeSsbo(string name) => Take("ssbo", TakenSsbo, name);
    public static int TakeUbo(string name) => Take("ubo", TakenUbo, name);
    public static int TakeTexUnit(string name) => Take("textureUnit", TakenTexUnits, name);
    
    // name for debug purpose
    private static int Take(string typeName, Dictionary<string, int> taken, string name)
    {
        if (taken.TryGetValue(name, out var binding))
            return binding;

        taken[name] = taken.Count;
        // Logger.Info(typeof(StaticBindings), $"Current bindings for {typeName}:");
        // foreach (var (key, bind) in taken) 
        //     Logger.Info(typeof(StaticBindings), $"{key} -> {bind}");

        return taken.Count - 1;
    }
    
}