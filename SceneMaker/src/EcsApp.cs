using System.ComponentModel;
using Enjune;
using Enjune.Misc;
using Enjune.Physics;
using Enjune.Physics.Component;
using Enjune.Physics.EcsType;
using Enjune.Physics.System;

namespace SceneMaker;

public class EcsApp : IApp
{
    public void Dispose()
    {
        return;
    }

    public Error? Init()
    {
        World.ComponentManager.RegisterComponentType<Enjune.Physics.Component.Position>()
            .RegisterComponentType<Velocity>()
            .RegisterComponentType<Acceleration>();
        
        World.SystemManager.RegisterSystem(new IntegrationSystem());
        World.SystemManager.RegisterSystem(new GravitySystem());
        
        ushort id = World.EntityManager.CreateEntity()!.Value;
        
        var testEntity = new EntityAssembly(id);
        testEntity.AddComponent(new Enjune.Physics.Component.Position(0, 0, 0));
        testEntity.AddComponent(new Velocity(0,0,0));
        testEntity.AddComponent(new Acceleration(0,0,0));
        
        World.ArchetypeManager.AddEntity(testEntity);
        
        return null;
    }

    public void Run()
    {
        Logger.Log(GetType(), "Starting the main loop");
        for (var i = 0; i < 1000; i ++)
        {
            World.SystemManager.Update<GravitySystem>();
            World.SystemManager.Update<IntegrationSystem>();
            Logger.Log(GetType(), $"Finished step {i + 1} of the loop");
        }
    }
}