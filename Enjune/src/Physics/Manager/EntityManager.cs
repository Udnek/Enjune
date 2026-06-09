using Enjune.Misc;
using Enjune.Physics.EcsType;

namespace Enjune.Physics.Manager;

public class EntityManager
{
    private readonly Stack<EntityId> _availableEntities = new();
    private readonly List<EntityId> _activeEntities = new();
    //private Signature[] _signatures =  new Signature[EcsConstants.MaxEntities];
    
    public EntityManager()
    {
        Logger.Log(GetType(), "registering entity IDs");
        Stack<EntityId> rawEntities = new();
        for (EntityId id = 0; id < EcsConstants.MaxEntities; id++)
        {
            rawEntities.Push(id);
        }
        // TODO: This reversal strategy sucks ass imo
        while (rawEntities.Count > 0)
        {
            _availableEntities.Push(rawEntities.Pop());
        }
        Logger.Log(GetType(), $"registered entity IDs. Range: [{_availableEntities.Peek()}; {_availableEntities.Last()}]");
    }

    private bool HasAvailableEntity() => _availableEntities.Count > 0;

    private static bool EntityIsValid(EntityId id) => id < EcsConstants.MaxEntities;

    private bool EntityIsLiving(EntityId id)
    {
        if (EntityIsValid(id))
        {
            return !_availableEntities.Contains(id);
        }
        return false;
    }
    
    public EntityId? CreateEntity()
    {
        if (!HasAvailableEntity())
        {
            Logger.Warn(GetType(), "new entity requested, but no more entities available! Ignoring request");
            return null;
        }
        EntityId id = _availableEntities.Pop();
        return id;
    }

    public void DestroyEntity(EntityId id)
    {
        if (!EntityIsLiving(id))
        {
            Logger.Warn(GetType(), "Invalid entity destruction requested! Ignoring request");
            return;
        }
        _availableEntities.Push(id);
    }

    /*public void SetSignature(EntityId id, Signature signature)
    {
        if (!EntityIsLiving(id))
        {
            Console.WriteLine("Tried changing signature for invalid entity! Ignoring request");
            return;
        }
        _signatures[id] = signature;
    }

    public Signature? GetSignature(EntityId id)
    {
        if (!EntityIsLiving(id))
        {
            Console.WriteLine("Tried getting signature for invalid entity! Ignoring request");
            return null;
        }
        return _signatures[id];
    }*/
}