using Enjune.Ecs;
using Minecraft2.Ecs.System;

namespace Minecraft2.Ecs;

public static class Systems
{
    public static void AddTo(World world, App app)
    {
        // loading, unloading and marking for removal
        world.AddSystem(new ChunkUnloadMarkerSystem {App = app});
        world.AddSystem(new ChunkLoadSystem {App = app});
        world.AddSystem(new ChunkRequestRemoveSystem {App = app});
        
        // adding and removing models
        world.AddSystem(new ChunkManageModelSystem {App = app});
        
        // rendering
        world.AddSystem(new ChunkRenderSystem {App = app});
        
        // removing marked entities
        world.AddSystem(new EntityRemoveSystem {App = app});
    }
}