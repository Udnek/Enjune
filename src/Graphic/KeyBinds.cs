using Enjune.Misc;

namespace Enjune.Graphic;

public static class KeyBinds
{
    private static readonly Dictionary<GlfwKey, Bind> Binds = new();

    public static readonly Bind Forward = BindKey(new Bind("forward", GlfwKey.W, true));
    public static readonly Bind Backward = BindKey(new Bind("backward", GlfwKey.S, true));
    public static readonly Bind Leftward = BindKey(new Bind("leftward", GlfwKey.A, true));
    public static readonly Bind Rightward = BindKey(new Bind("rightward", GlfwKey.D, true));

    public static readonly Bind Upward = BindKey(new Bind("upward", GlfwKey.Space, true));
    public static readonly Bind Downward = BindKey(new Bind("downward", GlfwKey.LeftShift, true));

    public static readonly Bind LookUp = BindKey(new Bind("look_up", GlfwKey.Up, true));
    public static readonly Bind LookDown = BindKey(new Bind("look_down", GlfwKey.Down, true));
    public static readonly Bind LookLeft = BindKey(new Bind("look_left", GlfwKey.Left, true));
    public static readonly Bind LookRight = BindKey(new Bind("look_right", GlfwKey.Right, true));

    public static readonly Bind DebugMenu = BindKey(new Bind("debug", GlfwKey.F3));
    public static readonly Bind DumpTextures = BindKey(new Bind("dump_textures", GlfwKey.F4));
    public static readonly Bind SwitchDrawMode = BindKey(new Bind("SwitchDrawMode", GlfwKey.F5));

    public static Bind? Get(GlfwKey key) => Binds.GetValueOrDefault(key);
    
    public static Bind BindKey(Bind bind)
    {
        if (Binds.TryGetValue(bind.GlfwKey, out var existed))
        {
            Logger.Warn(typeof(KeyBinds), $"rebound key from {existed} to {bind}");
        }
        Binds[bind.GlfwKey] = bind;
        return bind;
    }

    public record Bind(string Name, GlfwKey GlfwKey, bool ContinuousPress = false){}
}