using Enjune.Ecs.EcsType;
using Enjune.Misc;

namespace Enjune.Ecs.Manager;

public sealed class EntityManager
{
    private int _counter = 1;
    
    public EntityManager()
    {
        Logger.Info(this, $"Initialized at counter = {_counter}");
    }

    public Entity CreateEntity() => new(_counter++);
}