using Enjune.Ecs.EcsType;
using Enjune.Logger;
using Enjune.Misc;

namespace Enjune.Ecs.Manager;

[LogParams(logCallingMethod: true)]
public sealed class ArchetypeManager(World world)
{
    private readonly Dictionary<Signature, Archetype> _signatureToArchetype = new();
    private readonly Dictionary<Entity, Archetype> _entityToArchetype = new();
    private readonly World _world = world;

    private void EnsureArchetypeExistence(Signature signature)
    {
        if (_signatureToArchetype.ContainsKey(signature)) return;
        _signatureToArchetype[signature] = new Archetype(signature, _world);
    }

    /// <summary>
    /// Collects all <see cref="Archetype"/> instances that match a component filter
    /// </summary>
    /// <param name="include">Components that an archetype must have to be collected</param>
    /// <param name="exclude">Components that an archetype must not have to be collected</param>
    /// <returns>All matched <see cref="Archetype"/>s</returns>
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
        
        EnsureArchetypeExistence(signature);

        Archetype matchedArchetype = _signatureToArchetype[signature];
        matchedArchetype.AddEntity(assembly, entity);
        _entityToArchetype[entity] = matchedArchetype;
    }
    
    internal void RemoveEntity(Entity entity)
    {
        if (_entityToArchetype.TryGetValue(entity, out Archetype? archetype)) 
        {
            archetype.RemoveEntity(entity);
            _entityToArchetype.Remove(entity);
        }
        else
            Logger.Logger.Warn(this, $"Couldn't find {entity} to remove");
    }

    internal void MoveEntity(Entity entity, Archetype from, Archetype to)
    {
        to.AddEntity(entity, from.GetEntityComponents(entity));
        from.RemoveEntity(entity);
        _entityToArchetype[entity] = to;
        Logger.Logger.Debug(this, $"Moved {entity} from {from.Signature} to {to.Signature}");
    }

    internal Archetype GetArchetypeByEntity(Entity entity)
        => _entityToArchetype[entity];
    
    internal Archetype GetOrAddArchetypeBySignature(Signature signature)
    {
        EnsureArchetypeExistence(signature);
        return _signatureToArchetype[signature];
    }
        
}