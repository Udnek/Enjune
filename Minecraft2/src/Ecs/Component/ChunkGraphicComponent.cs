using Enjune.Ecs.Component;
using Enjune.Graphic.Modeling;
using Enjune.Logger;
using Enjune.Registering;

namespace Minecraft2.Ecs.Component;

public struct ChunkGraphicComponent() : IComponent
{
    public Guid GraphicId = Guid.NewGuid();
    public required ChunkModel Model;

    public byte BlockSize // basically LoD: bigger size -> lower quality
    {
        get;
        set
        {
            if (value == 0)
                Logger.Error(this, $"{nameof(BlockSize)} must be positive, got {value}");
            else
                field = value;
        }
    } = 1; 

    public bool BlockSizeChanged = false;
    public required byte PendingNeighbors = 0; // for mesh generation

    public Identifier Id() => Identifier.Of(Program.Assembly, "chunk_graphic");
}