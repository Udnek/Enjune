using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;

namespace Enjune.Ecs.Manager;

public sealed class SystemManager(World world)
{
    private readonly List<ISystem> _systems = [];
    private readonly World _world = world;

    public SystemManager RegisterSystem(ISystem system)
    {
        _systems.Add(system);
        system.Initialize(new Signature.Builder(_world));
        return this;
    }

    public void UpdateAll()
    {
        foreach (var system in _systems) 
            system.Update(_world);
    }
}