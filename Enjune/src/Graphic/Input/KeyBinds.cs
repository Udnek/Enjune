using Enjune.Misc;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.Input;

public sealed class KeyBinds
{
    private readonly Dictionary<UniKey, Bind> _binds = new();

    private KeyBinds(){}

    public static KeyBinds CreateEmpty() => new();

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

    public bool TryGet(UniKey key, out Bind bind) => _binds.TryGetValue(key, out bind);
    
    public Bind AddBind(Bind bind)
    {
        if (_binds.TryGetValue(bind.Key, out var existed)) 
            Logger.Warn(typeof(KeyBinds), $"rebound key from {existed} to {bind}");
        
        _binds[bind.Key] = bind;
        return bind;
    }

    public record struct Bind(
        string Name,
        UniKey Key,
        bool ContinuousPress = false)
    {

        public Bind(string name, GlfwKey glfwKey, bool continuousPress = false) 
            : this(name, UniKey.Of(glfwKey), continuousPress){}
        
        public Bind(string name, MouseButton mouseButton, bool continuousPress = false) 
            : this(name, UniKey.Of(mouseButton), continuousPress){}
    }
}













