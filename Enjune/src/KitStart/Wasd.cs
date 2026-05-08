using Enjune.Graphic.Key;
using static Enjune.Graphic.Key.KeyCode;

namespace Enjune.KitStart;

public record Wasd(
    KeyBinds.Bind Forward,
    KeyBinds.Bind Leftward,
    KeyBinds.Bind Backward,
    KeyBinds.Bind Rightward,
    KeyBinds.Bind Upward,
    KeyBinds.Bind Downward
)
{
    public static Wasd AddTo(KeyBinds keyBinds)
    {
        return new Wasd(
            keyBinds.AddBind(new KeyBinds.Bind("forward", W, true)),
            keyBinds.AddBind(new KeyBinds.Bind("leftward", A, true)),
            keyBinds.AddBind(new KeyBinds.Bind("backward", S, true)),
            keyBinds.AddBind(new KeyBinds.Bind("rightward", D, true)),
            keyBinds.AddBind(new KeyBinds.Bind("upward", Space, true)),
            keyBinds.AddBind(new KeyBinds.Bind("downward", LeftShift, true))
        );
    }
}