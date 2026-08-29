using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using System.Reflection.Metadata.Ecma335;

namespace Enjune.Ecs.Manager;

public sealed class ArchetypeManager(World world)
{
    private readonly Dictionary<Signature, Archetype> _signatureToArchetype = new();
    private readonly Dictionary<Entity, Archetype> _entityToArchetype = new();
    private readonly World _world = world;
    
    public void EnsureArchetypeExistence(Signature signature)
    {
        if (_signatureToArchetype.ContainsKey(signature)) return;
        _world.InvalidateArchetypeCache();
        _signatureToArchetype[signature] = new Archetype(signature, _world);
        Logger.Info(this, $"Created an archetype with signature {signature}");
    }
    
    public IEnumerable<Archetype> Query(Signature include, Signature exclude)
    {
        foreach (var (signature, archetype) in _signatureToArchetype)
        {
            if (signature.Includes(include) && signature.Excludes(exclude))
            {
                yield return archetype;
            }
        }
    }
    
    internal void AddEntity(Entity.Assembly assembly, Entity entity)
    {
        
        Signature signature = assembly.GetSignature(_world);
        Logger.Info(this, $"Got a request to add {entity} with signature {signature}");

        EnsureArchetypeExistence(signature);

        Archetype matchedArchetype = _signatureToArchetype[signature];
        matchedArchetype.AddEntity(assembly, entity);
        _entityToArchetype[entity] = matchedArchetype;
    }
    
    internal void RemoveEntity(Entity entity)
    {
        Logger.Info(this, $"Got a request to remove {entity}");
        if (_entityToArchetype.TryGetValue(entity, out Archetype? archetype)) 
        {
            archetype.RemoveEntity(entity);
            _entityToArchetype.Remove(entity);
        }
        else
            Logger.Warn(this, $"Can not remove entity; not found: {entity}");
    }

    internal void MoveEntity(Entity entity, Archetype from, Archetype to)
    {
        Logger.Info(this, $"{nameof(MoveEntity)}: Moving {entity} from {from.Signature} to {to.Signature}");
        to.AddEntity(entity, from.GetAllEntityComponents(entity));
        from.RemoveEntity(entity);
        _entityToArchetype[entity] = to;
    }

    internal Archetype GetArchetypeByEntity(Entity entity)
        => _entityToArchetype[entity];
    internal Archetype GetOrAddArchetypeBySignature(Signature signature)
    {
        EnsureArchetypeExistence(signature);
        return _signatureToArchetype[signature];
    }
        
}