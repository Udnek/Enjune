using Enjune.Ecs.Component;
using Enjune.Registering;
using Minecraft2.Misc;
using Minecraft2.Terrain;
using OpenTK.Mathematics;

namespace Minecraft2.Ecs.Component;

public record struct ChunkComponent() : IComponent
{
    public required Chunk Chunk { get; init; }
    public bool ToBeUnloaded = false;
    public required ChunkPos Pos { get; init; }

    public Identifier Id() => Identifier.Of(Program.Assembly, "chunk");
}