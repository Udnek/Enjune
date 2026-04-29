using Enjune.Misc;
using static Enjune.Graphic.Input.KeyCode;

namespace Enjune.Graphic.Input;

public sealed class KeyBinds
{
    private readonly Dictionary<KeyCode, Bind> _binds = new();

    private KeyBinds(){}

    public static KeyBinds CreateEmpty() => new();

    public static KeyBinds AddWasd(KeyBinds keyBinds, out Wasd wasd)
    {
        wasd = new Wasd(
            keyBinds.AddBind(new Bind("forward", W, true)),
            keyBinds.AddBind(new Bind("leftward", A, true)),
            keyBinds.AddBind(new Bind("backward", S, true)),
            keyBinds.AddBind(new Bind("rightward", D, true)),
            keyBinds.AddBind(new Bind("upward", Space, true)),
            keyBinds.AddBind(new Bind("downward", LeftShift, true))
        );
        return keyBinds;
    }

    public bool TryGet(KeyCode keyCode, out Bind? bind) => _binds.TryGetValue(keyCode, out bind);
    
    public Bind AddBind(Bind bind)
    {
        if (_binds.TryGetValue(bind.KeyCode, out var existed)) 
            Logger.Warn(typeof(KeyBinds), $"rebound key from {existed} to {bind}");
        
        _binds[bind.KeyCode] = bind;
        return bind;
    }

    public sealed record Bind(
        string Name,
        KeyCode KeyCode,
        bool ContinuousPress = false
        );
}













