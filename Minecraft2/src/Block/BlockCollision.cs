using Enjune.Misc;

namespace Minecraft2.Block;

public readonly struct BlockCollision
{
    public static readonly BlockCollision Full = new(Vector3.Zero, Vector3.One);
    public readonly Vector3 Min;
    public readonly Vector3 Max;

    public BlockCollision(Vector3 corner1, Vector3 corner2)
    {
        Min = Vector3.ComponentMin(corner1, corner2);
        Max = Vector3.ComponentMax(corner1, corner2);
    }

    public bool ContainsPoint(Vector3 point) => Min <= point && point <= Max;
}