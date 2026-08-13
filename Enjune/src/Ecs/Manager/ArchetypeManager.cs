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
        _world.InvalidateArchetypeCache();
        _archetypes[signature] = new Archetype(signature, _world);
        Logger.Info(this, $"Created an archetype with signature {signature}");
    }

    public void AddEntity(EntityAssembly assembly, Entity entity)
    {
        Signature signature = assembly.GetSignature(_world);
        Logger.Info(this, $"got a request to add an entity {entity} with signature {signature}");

        EnsureArchetypeExistence(signature);

        Archetype matchedArchetype = _archetypes[signature];
        matchedArchetype.AddEntity(assembly, entity);
        _entityToArchetype[entity] = matchedArchetype;
    }

    public Archetype GetArchetype(Signature signature)
    {
        if (_archetypes.TryGetValue(signature, out var archetype)) 
            return archetype;
        archetype = new Archetype(signature, _world);
        _archetypes.Add(signature, archetype);
        return archetype;
    }

    public void RemoveEntity(Entity entity)
    {
        Logger.Info(this, $"Got a request to remove entity {entity}");
        if (_entityToArchetype.TryGetValue(entity, out Archetype? archetype)) 
            archetype.RemoveEntity(entity);
        else
            Logger.Warn(this, $"Can not remove entity; not found: {entity}");
    }

    public IEnumerable<Archetype> Query(Signature includeSignature, Signature excludeSignature)
    {
        foreach (var (signature, archetype) in _archetypes)
        {
            if (signature.Includes(includeSignature) && signature.Excludes(excludeSignature))
            {
                yield return archetype;
            }
        }
    }

    public IEnumerable<Archetype> QuerySimple(Signature signature)
    {
        return Query(signature, Signature.Empty);
    }
}