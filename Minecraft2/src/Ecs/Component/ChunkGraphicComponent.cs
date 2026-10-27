using Enjune.Ecs.Component;
using Enjune.Graphic.Modeling;
using Enjune.Registering;

namespace Minecraft2.Ecs.Component;

public struct ChunkGraphicComponent() : IComponent
{
    public Guid GraphicId = Guid.NewGuid();
    public required ChunkModel Model;
    public int BlockSize = 1; // basically LoD: bigger size -> lower quality
    public bool BlockSizeChanged = false;
    public required int PendingNeighbors = 0; // for mesh generation

    public Identifier Id() => Identifier.Of(Program.Assembly, "chunk_graphic");
}