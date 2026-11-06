using Enjune.Ecs.System;
using Enjune.Logger;

namespace Enjune.Ecs.Manager;

public sealed class SystemManager(World world)
{
    private readonly World _world = world;
    private readonly List<ISystem> _systems = [];

    public void RegisterSystem(ISystem system)
    {
        _systems.Add(system);
        system.OnInit(_world);
        Logger.Logger.Info(this, $"Registered {system} at position {_systems.Count - 1}");
    }

    public void UpdateAll()
    {
        foreach (var system in _systems) 
            system.OnUpdate();
    }
}