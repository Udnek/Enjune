namespace Enjune.Graphic;

public record Wasd(
    KeyBinds.Bind Forward,
    KeyBinds.Bind Leftward,
    KeyBinds.Bind Backward,
    KeyBinds.Bind Rightward,
    KeyBinds.Bind Upward,
    KeyBinds.Bind Downward
    );