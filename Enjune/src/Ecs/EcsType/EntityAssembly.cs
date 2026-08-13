using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Misc;

namespace Enjune.Ecs.EcsType;

// EntityAssembly's purpose is not only to conveniently create and move entities,
// but to also make sure that an entity always has correct structure. 
// This class should NOT be used in big loops to store and manage entities
public sealed class EntityAssembly
{
    private readonly Dictionary<Type, IComponent> _components = new();
    
    public List<IComponent> GetComponents() => _components.Values.ToList();
    public List<Type> GetComponentTypes() => _components.Keys.ToList(); 

    public EntityAssembly AddComponent(IComponent component)
    {
        if (_components.ContainsKey(component.GetType())) 
            Logger.Warn(this, $"Replaced component {component.GetType()} for an assembly");
        _components.Add(component.GetType(), component);

        return this;
    }

    public void RemoveComponent(IComponent component) 
        => _components.Remove(component.GetType());

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