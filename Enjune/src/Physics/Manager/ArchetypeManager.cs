using Enjune.Physics.Component;
using Enjune.Physics.EcsType;

namespace Enjune.Physics.Manager;

public class ArchetypeManager
{
    private Dictionary<Signature, Archetype> _archetypes = new();
    private IComponent[] _components;

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