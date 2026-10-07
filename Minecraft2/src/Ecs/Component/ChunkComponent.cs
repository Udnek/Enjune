using Enjune.Ecs.Component;
using Enjune.Registering;
using Minecraft2.Misc;
using OpenTK.Mathematics;

namespace Minecraft2.Ecs.Component;

public struct ChunkComponent() : IComponent
{
    public required Chunk Chunk;
    public bool ToBeUnloaded = false;
    public required Vector3i Pos;

    public Identifier Id() => Identifier.Of(Program.Assembly, "chunk");
}