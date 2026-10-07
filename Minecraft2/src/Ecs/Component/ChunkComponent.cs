using Enjune.Ecs.Component;
using Enjune.Registering;
using Minecraft2.Misc;

namespace Minecraft2.Ecs.Component;

public struct ChunkComponent() : IComponent
{
    public required Chunk Chunk;
    
    public Identifier Id() => Identifier.Of(Program.Assembly, "chunk");
}