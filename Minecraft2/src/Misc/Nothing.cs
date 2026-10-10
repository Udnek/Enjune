namespace Minecraft2.Misc;

public readonly record struct Nothing
{
    public static readonly Nothing Instance = default;

    public override string ToString() => nameof(Nothing);
}