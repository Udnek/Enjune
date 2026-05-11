using Enjune.Misc;
using Enjune.Physics.Component;
using Enjune.Physics.EcsType;

namespace Enjune.Physics.Manager;

public class ArchetypeManager
{
    private Dictionary<Signature, Archetype> _archetypes = new();
    private IComponent[] _components;

    public void EnsureArchetypeExistence(Signature signature)
    {
        if (!_archetypes.ContainsKey(signature))
        {
            _archetypes[signature] = new Archetype(signature);
            Logger.Log(GetType(), $"Archetype with signature {signature} created");
        }
    }

    public void AddEntity(EntityAssembly assembly)
    {
        Signature signature = assembly.GetSignature();
        Logger.Log(GetType(), $"Got a request to add an entity {assembly.Id} with signature {signature}");
        foreach (var archetypeSignature in _archetypes.Keys)
        {
            if (signature.Contains(archetypeSignature))
            {
                _archetypes[archetypeSignature].AddEntity(assembly);
            }
        }
    }

    public Archetype GetArchetype(Signature signature)
    {
        if (!_archetypes.TryGetValue(signature, out Archetype? archetype))
        {
            archetype = new Archetype(signature);
            _archetypes.Add(signature, archetype);
        }
        return archetype;
    }
}