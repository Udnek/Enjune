using System.Diagnostics.CodeAnalysis;

namespace Minecraft2.Block;

public class BlockType
{
    public required BlockRender? Render { get; init; }
    public required BlockCollision? Collision { get; init; }

    public readonly BlockState DefaultState;

    public BlockType() => DefaultState = new BlockState(this);
}