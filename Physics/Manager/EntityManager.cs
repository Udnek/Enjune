using System.Diagnostics;

namespace Enjune.Physics;

public class EntityManager
{
    private readonly Queue<EntityId> _availableEntities;
    private Signature[] _signatures =  new Signature[EcsConstants.MaxEntities];
    
    public EntityManager()
    {
        _availableEntities = new Queue<EntityId>();
        for (EntityId id = 0; id < EcsConstants.MaxEntities; id++)
        {
            _availableEntities.Enqueue(id);
        }
    }

    private bool HasAvailableEntity() { return _availableEntities.Count > 0; }

    private bool EntityIsValid(EntityId id) { return id < EcsConstants.MaxEntities; }

    private bool EntityIsLiving(EntityId id)
    {
        if (EntityIsValid(id))
        {
            return !_availableEntities.Contains(id);
        }
        return false;
    }
    
    public EntityId? CreateEntity(Signature signature)
    {
        if (!HasAvailableEntity())
        {
            Console.WriteLine("New entity requested, but no more entities available! Ignoring request");
            return null;
        }
        EntityId id = _availableEntities.Dequeue();
        SetSignature(id, signature);
        return id;
    }

    public void DestroyEntity(EntityId id)
    {
        if (!EntityIsLiving(id))
        {
            Console.WriteLine("Invalid entity destruction requested! Ignoring request");
            return;
        }
        _availableEntities.Enqueue(id);
    }

    public void SetSignature(EntityId id, Signature signature)
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
    }
}