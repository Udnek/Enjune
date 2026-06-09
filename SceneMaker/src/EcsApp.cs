using System.ComponentModel;
using Enjune;
using Enjune.Ecs;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using SceneMaker.Ecs.Component;
using SceneMaker.Ecs.System;

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
        List<ISystem> systems =
        [
            new IntegrationSystem(),
            new GravitySystem()
        ];

        List<Type> componentTypes =
        [
            typeof(Ecs.Component.Position),
            typeof(Velocity),
            typeof(Acceleration)
        ];

        _world = new World(systems, componentTypes);

        EntityId id = _world.GetNewEntityId();
        
        var testEntity = new EntityAssembly(id);
        testEntity.AddComponent(new Ecs.Component.Position(0, 0, 0));
        testEntity.AddComponent(new Velocity(0,0,0));
        testEntity.AddComponent(new Acceleration(0,0,0));
        
        _world.AddEntity(testEntity);
        
        return null;
    }

    public void MainCycle()
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