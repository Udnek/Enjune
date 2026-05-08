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
            // TODO: Send to logger
            Console.WriteLine("WARNING: Replaced a component that already existed");
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
}