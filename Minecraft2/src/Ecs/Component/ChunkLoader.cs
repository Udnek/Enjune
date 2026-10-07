using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Registering;

namespace Minecraft2.Ecs.Component;

public struct ChunkLoader() : IComponent
{
    public static readonly MapCodec<ChunkLoader> Codec = Codecs
        .ForEmptyConstructor(() => new ChunkLoader())
        .ForField("radius", i => i.Radius, (ref i, v) => i.Radius = v, Codecs.Int).Build();
    
    public int Radius = 8;

    public Identifier Id() => Identifier.Of(Program.Assembly, "chunk_loader");
}