using System.ComponentModel;
using Enjune;
using Enjune.Ecs;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using Standoff2.Ecs.System;
using Standoff2.Ecs.Component;
using System.Runtime.CompilerServices;

public class EcsTestApp : AbstractDisposable, IApp
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
            typeof(Position),
            typeof(Velocity),
            typeof(Acceleration),
            typeof(Mass)
        ];

        _world = new World(systems, componentTypes);
        
        return null;
    }

    public void MainCycle()
    {
        void UpdateTimes(int n)
        {
            for (int i = 0; i < n; i++)
            {
                _world.Update();
            }
        }

        var testEntityAssembly = new Entity.Assembly()
            .AddComponent(new Position(0, 0, 0))
            .AddComponent(new Velocity(0, 0, 0))
            .AddComponent(new Acceleration(0, 0, 0));

        Entity entity = _world.AddEntity(testEntityAssembly);

        Logger.Info(this, "---- SIMPLE LOOP TEST ----");
        Logger.Info(this, "Starting the main loop");
        DateTime startTime = DateTime.Now;
        for (var i = 0; i < 50; i ++)
        {
            _world.Update();
            Logger.Info(this, $"Finished step {i + 1} of the loop");
        }
        DateTime endTime = DateTime.Now;
        Logger.Info(this, $"Simulation took {endTime - startTime}");
        Logger.Info(this, "---- ENTITY REMOVAL TEST ----");
        Logger.Info(this, $"Removing {entity} as a test");
        _world.RemoveEntity(entity);
        Logger.Info(this, $"Updating world once again");
        _world.Update();

        Logger.Info(this, "---- COMPONENT ADDITION TEST ----");
        testEntityAssembly = new Entity.Assembly()
            .AddComponent(new Position(0, 0, 0))
            .AddComponent(new Velocity(0, 0, 0));
        Logger.Info(this, $"Adding an entity without an acceleration");
        entity = _world.AddEntity(testEntityAssembly);
        Logger.Info(this, "Updating world");
        _world.Update();

        Logger.Info(this, $"Adding acceleration to {entity}");
        _world.AddEntityComponent(entity, new Acceleration(0,0,0));
        Logger.Info(this, "Updating world");
        _world.Update();

        Logger.Info(this, $"Adding mass to {entity}");
        _world.AddEntityComponent(entity, new Mass(10));
        Logger.Info(this, "Updating world");
        _world.Update();

        Logger.Info(this, $"Removing {entity}");
        _world.RemoveEntity(entity);
        Logger.Info(this, "Updating world");
        _world.Update();

        Logger.Info(this, "---- COMPONENT REMOVAL TEST ----");
        testEntityAssembly = new Entity.Assembly()
            .AddComponent(new Position())
            .AddComponent(new Velocity())
            .AddComponent(new Acceleration())
            .AddComponent(new Mass());
        Logger.Info(this, $"Adding an entity with all components");
        entity = _world.AddEntity(testEntityAssembly);
        Logger.Info(this, "Updating world");
        _world.Update();

        Logger.Info(this, $"Removing mass from {entity}");
        _world.RemoveEntityComponent<Mass>(entity);
        Logger.Info(this, "Updating world");
        _world.Update();

        Logger.Info(this, $"Removing acceleration from {entity}");
        _world.RemoveEntityComponent<Acceleration>(entity);
        Logger.Info(this, "Updating world");
        _world.Update();

        Logger.Info(this, $"Removing {entity}");
        _world.RemoveEntity(entity);
        Logger.Info(this, "Updating world");
        _world.Update();

        Logger.Info(this, "---- ENTITY-ACCESS METHODS TEST ----");
        testEntityAssembly = new Entity.Assembly()
            .AddComponent(new Position(0,0,0));
        Logger.Info(this, $"Adding an entity with Position(0,0,0)");
        entity = _world.AddEntity(testEntityAssembly);
        Logger.Info(this, "Updating world");
        _world.Update();

        Logger.Info(this, $"Changing {entity} position to 100 0 100");
        var optRef = _world.ModifyEntityComponent<Position>(entity, pos => {
            pos.X = 100;
            pos.Z = 100;
            return pos;
        });
        Logger.Info(this, $"Adding remaining components");
        _world.AddEntityComponent(entity, new Velocity(0, 0, 0));
        _world.AddEntityComponent(entity, new Acceleration(0, 0, 0));
        _world.AddEntityComponent(entity, new Mass(0));

        Logger.Info(this, "Updating world 3 times");
        UpdateTimes(3);

        Logger.Info(this, $"Removing position");
        _world.RemoveEntityComponent<Position>(entity);

        Logger.Info(this, "Updating world 3 times");
        UpdateTimes(3);

        Logger.Info(this, $"Getting {entity} velocity");
        Logger.Info(this, $"{_world.GetEntityComponent<Velocity>(entity)}");

        Logger.Info(this, "Operation finished");
    }

    protected override void DisposeData() { }
}