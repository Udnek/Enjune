using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Registering;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs;

public static class Components
{
    public static void Boot()
    {
        Register(new Transform(), Transform.Codec);
        Register(new ChunkLoader(), ChunkLoader.Codec);
    }

    private static void Register<T>(T comp, ICodec<T> codec) where T : IComponent
    {
        Registries.Codec.Register(comp.Id(), codec);
    }
}