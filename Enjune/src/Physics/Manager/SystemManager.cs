using Enjune.Misc;
using Enjune.Physics.System;

namespace Enjune.Physics.Manager;

public class SystemManager
{
    private Dictionary<Type, ISystem> _systems;

    public SystemManager()
    {
        _systems = new Dictionary<Type, ISystem>();
    }
    
    // TODO:
    // Returning SystemManager allows for chain building,
    // however such syntax looks dirty and upgradable
    public SystemManager RegisterSystem(ISystem system)
    {
        _systems.Add(system.GetType(), system);
        return this;
    }

    public void Update<TSystem>() where TSystem : ISystem
    {
        _systems.TryGetValue(typeof(TSystem), out var system);
        if (system != null)
        {
            system.Update();
            return;
        }
        Logger.Warn(GetType(), $"system {typeof(TSystem).Name} was not registered before updating");
    }
}