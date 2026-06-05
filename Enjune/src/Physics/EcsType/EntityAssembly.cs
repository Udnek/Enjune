using System.Runtime.InteropServices;
using Enjune.Misc;
using Enjune.Physics.Component;

namespace Enjune.Physics.EcsType;

// EntityAssembly's purpose is not only to conveniently create and move entities,
// but to also make sure that an entity always has correct structure. 
// This class should NOT be used in big loops to store and manage entities
public class EntityAssembly
{
    public readonly EntityId Id;
    private readonly Dictionary<Type, IComponent> _components = new();
    
    public EntityAssembly(EntityId id)
    {
        Id = id;
    }
    
    public List<IComponent> GetComponents() => _components.Values.ToList();
    public List<Type> GetComponentTypes() => _components.Keys.ToList(); 

    public void AddComponent(IComponent component)
    {
        if (_components.ContainsKey(component.GetType()))
        {
            Logger.Warn(this, $"replaced component {component.GetType()} for assembly {Id}");
        }
        _components.Add(component.GetType(), component);
    }

    public void RemoveComponent(IComponent component)
    {
        _components.Remove(component.GetType());
    }
    
    public IComponent? GetComponent(Type componentType)
    {
        _components.TryGetValue(componentType, out IComponent? component);
        return component;
    }

    public Signature GetSignature(World world)
    {
        var signatureBuilder = new SignatureBuilder(world);
        foreach (Type componentType in GetComponentTypes())
        {
            signatureBuilder.RegisterComponent(componentType);
        }
        return signatureBuilder.Build();
    }
}