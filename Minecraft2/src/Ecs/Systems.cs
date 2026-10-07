using Enjune.Ecs;
using Minecraft2.Ecs.System;

namespace Minecraft2.Ecs;

public static class Systems
{
    public static void AddTo(World world, App app)
    {
        // marking all for unloading
        world.AddSystem(new ChunkUnloadMarkerSystem {App = app});
        // loading and unmarking
        world.AddSystem(new ChunkLoadSystem {App = app});
        // unloading marked
        world.AddSystem(new ChunkUnloadSystem {App = app});
        
        // removing and adding models
        world.AddSystem(new ChunkRemoveModelSystem {App = app});
        world.AddSystem(new ChunkAddModelSystem {App = app});
        
        // rendering
        world.AddSystem(new ChunkRenderSystem {App = app});
        
        // removing marked entities
        world.AddSystem(new EntityRemoveSystem {App = app});
    }
}