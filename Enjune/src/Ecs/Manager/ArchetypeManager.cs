using Enjune.Ecs.EcsType;
using Enjune.Misc;
using System.Reflection.Metadata.Ecma335;

namespace Enjune.Ecs.Manager;

public sealed class ArchetypeManager
{
    private readonly Dictionary<Signature, Archetype> _archetypes = new();
    private readonly Dictionary<Entity, Archetype> _entityToArchetype = new();
    private readonly World _world;
    
    public ArchetypeManager(World world) => _world = world;

    private void EnsureArchetypeExistence(Signature signature)
    {
        if (_archetypes.ContainsKey(signature)) return;
        
        _archetypes[signature] = new Archetype(signature, _world);
        Logger.Info(this, $"Created an archetype with signature {signature}");
        _world.InvalidateCache();
    }

    public void AddEntity(EntityAssembly assembly, EntityId id)
    {
        Signature signature = assembly.GetSignature(_world);
        Logger.Info(this, $"got a request to add an entity {id} with signature {signature}");

        EnsureArchetypeExistence(signature);

        Archetype matchedArchetype = _archetypes[signature];
        matchedArchetype.AddEntity(assembly, id);
        _entityToArchetype[id] = matchedArchetype;
    }

    public Archetype GetArchetype(Signature signature)
    {
        if (_archetypes.TryGetValue(signature, out var archetype)) 
            return archetype;
        archetype = new Archetype(signature, _world);
        _archetypes.Add(signature, archetype);
        return archetype;
    }

    public void ForEachMatched(Signature signature, Action<Archetype> action)
    {
        foreach (var archetype in _archetypes.Values)
        {
            if (archetype.Signature.Contains(signature)) 
                action(archetype);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        Logger.Info(this, $"Got a request to remove entity {entity}");
        if (_entityToArchetype.TryGetValue(entity, out Archetype? archetype)) 
            archetype.RemoveEntity(entity);
        else
            Logger.Warn(this, $"Can not remove entity; not found: {entity}");
    }
}