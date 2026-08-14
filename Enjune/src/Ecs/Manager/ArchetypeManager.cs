using Enjune.Ecs.EcsType;
using Enjune.Misc;
using System.Reflection.Metadata.Ecma335;

namespace Enjune.Ecs.Manager;

public sealed class ArchetypeManager(World world)
{
    private readonly Dictionary<Signature, Archetype> _archetypes = new();
    private readonly Dictionary<Entity, Archetype> _entityToArchetype = new();
    private readonly World _world = world;
    
    private void EnsureArchetypeExistence(Signature signature)
    {
        if (_archetypes.ContainsKey(signature)) return;
        _world.InvalidateArchetypeCache();
        _archetypes[signature] = new Archetype(signature, _world);
        Logger.Info(this, $"Created an archetype with signature {signature}");
    }
    
    public IEnumerable<Archetype> Query(Signature include, Signature exclude)
    {
        foreach (var (signature, archetype) in _archetypes)
        {
            if (signature.Includes(include) && signature.Excludes(exclude))
            {
                yield return archetype;
            }
        }
    }
    
    public void AddEntity(Entity.Assembly assembly, Entity entity)
    {
        Signature signature = assembly.GetSignature(_world);
        Logger.Info(this, $"got a request to add an entity {entity} with signature {signature}");

        EnsureArchetypeExistence(signature);

        Archetype matchedArchetype = _archetypes[signature];
        matchedArchetype.AddEntity(assembly, entity);
        _entityToArchetype[entity] = matchedArchetype;
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