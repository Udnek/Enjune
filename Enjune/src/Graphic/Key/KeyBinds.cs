using Enjune.Misc;

namespace Enjune.Graphic.Key;

public sealed class KeyBinds
{
    private readonly Dictionary<KeyCode, Bind> _binds = new();

    private KeyBinds(){}

    public static KeyBinds CreateEmpty() => new();
    
    public bool TryGet(KeyCode keyCode, out Bind bind) => _binds.TryGetValue(keyCode, out bind);
    
    // TODO does this system even needed???
    public Bind AddBind(Bind bind)
    {
        if (_binds.TryGetValue(bind.KeyCode, out var existed)) 
            Logger.Warn(typeof(KeyBinds), $"rebound key from {existed} to {bind}");
        
        _binds[bind.KeyCode] = bind;
        return bind;
    }

    public readonly record struct Bind(
        string Name,
        KeyCode KeyCode,
        bool ContinuousPress = false
        );
}













