using Enjune.Misc;
using Enjune.Physics.EcsType;
using Enjune.Physics.System;

namespace Enjune.Physics.Manager;

public class SystemManager
{
    private Dictionary<Type, ISystem> _systems = new();
    private World _world;
    
    public SystemManager(World world)
    {
        _world = world;
    }

    // TODO:
    // Returning SystemManager allows for chain building,
    // however such syntax looks dirty and upgradable
    public SystemManager RegisterSystem(ISystem system)
    {
        _systems.Add(system.GetType(), system);
        return this;
    }

    public void InitializeSystems()
    {
        foreach (var system in _systems.Values)
        {
            system.Initialize(new SignatureBuilder(_world));
        }
    }

    // TODO update them all in order of registering -- ok boss
    public void Update<TSystem>(World world) where TSystem : ISystem
    {
        _systems.TryGetValue(typeof(TSystem), out var system);
        if (system != null)
        {
            system.Update(world);
            return;
        }
        Logger.Warn(GetType(), $"system {typeof(TSystem).Name} was not registered before updating");
    }
}