using Enjune.Physics.Component;
using Enjune.Physics.EcsType;

namespace Enjune.Physics.Manager;

public class ArchetypeManager
{
    private Dictionary<Signature, Archetype> _archetypes = new();
    private IComponent[] _components;

    //public void AddEntity(EntityId id, Span<IComponent> components)
    //{
    //    Signature signature = World.ComponentManager.ConstructSignature(components);
    //    if (!_archetypes.TryGetValue(signature, out Archetype? archetype))
    //    {
    //        archetype = new Archetype(signature);
    //        archetype.AddEntity(id, components);
    //        _archetypes.Add(signature, archetype); 
    //    }
    //}

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