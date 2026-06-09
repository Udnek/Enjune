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
    private World _world;
    public void Dispose()
    {
        return;
    }

    public Error? Init()
    {
        _world = new World();
        _world.ComponentManager.RegisterComponentType<Enjune.Physics.Component.Position>()
            .RegisterComponentType<Velocity>()
            .RegisterComponentType<Acceleration>();
        
        _world.SystemManager.RegisterSystem(new IntegrationSystem());
        _world.SystemManager.RegisterSystem(new GravitySystem());
        
        ushort id = _world.EntityManager.CreateEntity()!.Value;
        
        var testEntity = new EntityAssembly(id);
        testEntity.AddComponent(new Enjune.Physics.Component.Position(0, 0, 0));
        testEntity.AddComponent(new Velocity(0,0,0));
        testEntity.AddComponent(new Acceleration(0,0,0));
        
        _world.ArchetypeManager.AddEntity(testEntity);
        
        _world.Initialize();
        return null;
    }

    public void Run()
    {
        Logger.Log(GetType(), "Starting the main loop");
        DateTime startTime = DateTime.Now;
        for (var i = 0; i < 1000; i ++)
        {
            _world.Update<GravitySystem>();
            _world.Update<IntegrationSystem>();
            Logger.Log(GetType(), $"Finished step {i + 1} of the loop");
        }
        DateTime endTime = DateTime.Now;
        Logger.Log(GetType(), $"Simulation took {endTime - startTime}");
    }
}