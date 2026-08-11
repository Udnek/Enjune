using Enjune.Misc;

namespace Enjune.Ecs.Manager;

public sealed class EntityManager
{
    private EntityId _counter = 0;
    private readonly List<EntityId> _activeEntities = new();
    
    public EntityManager()
    {
        Logger.Info(this, $"EntityManager initialized at counter = {_counter}");
    }

    public EntityId CreateEntity()
    {
        return _counter++;
    }
}