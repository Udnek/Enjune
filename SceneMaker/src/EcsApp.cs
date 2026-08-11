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

public class EcsApp : AbstractDisposable, IApp
{
    private World _world = null!;
    
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
        
        var testEntity = new EntityAssembly()
            .AddComponent(new Ecs.Component.Position(0, 0, 0))
            .AddComponent(new Velocity(0,0,0))
            .AddComponent(new Acceleration(0,0,0));
        
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
        Logger.Log(this, $"Removing entity as a test");
        _world.RemoveEntity(0);
        Logger.Log(this, $"Updating world once again");
        _world.Update();
    }

    protected override void DisposeData() { }
}