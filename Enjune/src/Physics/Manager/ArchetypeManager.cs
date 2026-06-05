using Enjune.Misc;
using Enjune.Physics.Component;
using Enjune.Physics.EcsType;

namespace Enjune.Physics.Manager;

public class ArchetypeManager
{
    private Dictionary<Signature, Archetype> _archetypes = new();
    private World _world;
    
    public ArchetypeManager(World world)
    {
        _world = world;
    }

    public void EnsureArchetypeExistence(Signature signature)
    {
        if (!_archetypes.ContainsKey(signature))
        {
            _archetypes[signature] = new Archetype(signature, _world);
            Logger.Log(this, $"archetype with signature {signature} created");
        }
    }

    public void AddEntity(EntityAssembly assembly)
    {
        Signature signature = assembly.GetSignature(_world);
        Logger.Log(this, $"got a request to add an entity {assembly.Id} with signature {signature}");
        EnsureArchetypeExistence(signature);
        _archetypes[signature].AddEntity(assembly);
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

    public List<Archetype> GetArchetypes( )
    {
        // TODO use consumer and probably cache .Values???
        return _archetypes.Values.ToList();
    }

    public void Query(Signature signature, Action<Archetype> action)
    {
        foreach (Archetype archetype in _archetypes.Values)
        {
            if (archetype.Signature.Contains(signature))
            {
                action(archetype);
            }
        }
    }
}