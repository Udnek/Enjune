using System.ComponentModel;
using Enjune;
using Enjune.Ecs;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;

namespace SceneMaker;

public class EcsApp : IApp
{
    private World _world;
    
    public void Dispose()
    {
        return;
    }

    public Error? Init()
    {
        _world = new World();
        
        _world.ComponentManager
            .RegisterComponentType<Enjune.Ecs.Component.Position>()
            .RegisterComponentType<Velocity>()
            .RegisterComponentType<Acceleration>();
        
        _world.SystemManager.RegisterSystem(new IntegrationSystem());
        _world.SystemManager.RegisterSystem(new GravitySystem());
        
        ushort id = _world.EntityManager.CreateEntity()!.Value;
        
        var testEntity = new EntityAssembly(id);
        testEntity.AddComponent(new Enjune.Ecs.Component.Position(0, 0, 0));
        testEntity.AddComponent(new Velocity(0,0,0));
        testEntity.AddComponent(new Acceleration(0,0,0));
        
        _world.ArchetypeManager.AddEntity(testEntity);
        
        return null;
    }

    public void Run()
    {
        Logger.Log(this, "Starting the main loop");
        DateTime startTime = DateTime.Now;
        for (var i = 0; i < 1000; i ++)
        {
            _world.Update();
            Logger.Log(this, $"Finished step {i + 1} of the loop");
        }
        DateTime endTime = DateTime.Now;
        Logger.Log(this, $"Simulation took {endTime - startTime}");
    }
}