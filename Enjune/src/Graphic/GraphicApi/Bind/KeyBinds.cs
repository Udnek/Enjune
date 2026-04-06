using Enjune.Misc;

namespace Enjune.Graphic;

public sealed class KeyBinds
{
    private readonly Dictionary<GlfwKey, Bind> _binds = new();

    private KeyBinds(){}

    public static KeyBinds CreateEmpty() => new KeyBinds();

    public static KeyBinds AddWasd(KeyBinds keyBinds, out Wasd wasd)
    {
        wasd = new Wasd(
            keyBinds.AddBind(new Bind("forward", GlfwKey.W, true)),
            keyBinds.AddBind(new Bind("leftward", GlfwKey.A, true)),
            keyBinds.AddBind(new Bind("backward", GlfwKey.S, true)),
            keyBinds.AddBind(new Bind("rightward", GlfwKey.D, true)),
            keyBinds.AddBind(new Bind("upward", GlfwKey.Space, true)),
            keyBinds.AddBind(new Bind("downward", GlfwKey.LeftShift, true))
        );
        return keyBinds;
    }
    
    
    // public static readonly Bind LookUp = BindKey(new Bind("look_up", GlfwKey.Up, true));
    // public static readonly Bind LookDown = BindKey(new Bind("look_down", GlfwKey.Down, true));
    // public static readonly Bind LookLeft = BindKey(new Bind("look_left", GlfwKey.Left, true));
    // public static readonly Bind LookRight = BindKey(new Bind("look_right", GlfwKey.Right, true));
    //
    // public static readonly Bind DebugMenu = BindKey(new Bind("debug", GlfwKey.F3));
    // public static readonly Bind DumpTextures = BindKey(new Bind("dump_textures", GlfwKey.F4));
    // public static readonly Bind SwitchDrawMode = BindKey(new Bind("SwitchDrawMode", GlfwKey.F5));

    public Bind? Get(GlfwKey key) => _binds.GetValueOrDefault(key);
    
    public Bind AddBind(Bind bind)
    {
        if (_binds.TryGetValue(bind.GlfwKey, out var existed)) 
            Logger.Warn(typeof(KeyBinds), $"rebound key from {existed} to {bind}");
        
        _binds[bind.GlfwKey] = bind;
        return bind;
    }

    public record Bind(string Name, GlfwKey GlfwKey, bool ContinuousPress = false){}
}