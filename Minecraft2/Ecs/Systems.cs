using Enjune.Ecs;
using Minecraft2.Ecs.System;

namespace Minecraft2.Ecs;

public static class Systems
{
    public static void AddTo(World world, App app)
    {
        world.AddSystem(new ChunkLoadSystem());
        world.AddSystem(new ChunkAddModelSystem(app));
        world.AddSystem(new ChunkRenderSyncSystem(app));
    }
}