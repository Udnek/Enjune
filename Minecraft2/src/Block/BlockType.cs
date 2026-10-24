namespace Minecraft2.Block;

public class BlockType
{
    public required BlockRender? Render { get; init; }
    public required BlockCollision? Collision { get; init; }
}