using Enjune.Ecs.EcsType;
using Enjune.Misc;

namespace Enjune.Ecs.Manager;

public sealed class ArchetypeManager
{
    private readonly Dictionary<Signature, Archetype> _archetypes = new();
    private readonly Dictionary<EntityId, Archetype> _entityIdToArchetype = new();
    private readonly World _world;
    
    public ArchetypeManager(World world)
    {
        _world = world;
    }

    private void EnsureArchetypeExistence(Signature signature)
    {
        if (_archetypes.ContainsKey(signature)) return;
        
        _archetypes[signature] = new Archetype(signature, _world);
        Logger.Log(this, $"archetype with signature {signature} created");
    }

    public void AddEntity(EntityAssembly assembly)
    {
        Signature signature = assembly.GetSignature(_world);
        Logger.Log(this, $"got a request to add an entity {assembly.Id} with signature {signature}");
        EnsureArchetypeExistence(signature);
        Archetype matchedArchetype = _archetypes[signature];
        matchedArchetype.AddEntity(assembly);
        _entityIdToArchetype[assembly.Id] = matchedArchetype;
    }

    public Archetype GetArchetype(Signature signature)
    {
        if (!_archetypes.TryGetValue(signature, out Archetype? archetype))
        {
            archetype = new Archetype(signature, _world);
            _archetypes.Add(signature, archetype);
        }
        return archetype;
    }

    public void Query(Signature signature, Action<Archetype> action)
    {
        foreach (var archetype in _archetypes.Values)
        {
            if (archetype.Signature.Contains(signature)) 
                action(archetype);
        }
    }

    public void RemoveEntity(EntityId id)
    {
        
    }
}