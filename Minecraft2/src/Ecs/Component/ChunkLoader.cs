using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Registering;

namespace Minecraft2.Ecs.Component;

public struct ChunkLoader() : IComponent
{
    public static readonly MapCodec<ChunkLoader> Codec = Codecs
        .ForEmptyConstructor(() => new ChunkLoader())
        .ForField("horizontal_radius", i => i.HorizontalRadius, (ref i, v) => i.HorizontalRadius = v, Codecs.Int)
        .ForField("vertical_radius", i => i.VerticalRadius, (ref i, v) => i.VerticalRadius = v, Codecs.Int).Build();
    
    public int HorizontalRadius = 1;
    public int VerticalRadius = 1;

    public Identifier Id() => Identifier.Of(Program.Assembly, "chunk_loader");
}