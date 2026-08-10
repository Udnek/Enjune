using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Misc;

namespace Enjune.Ecs.Manager;

public sealed class ComponentManager
{
    private readonly Dictionary<Type, ComponentTypeId> _componentTypeIds = new();
    private readonly Dictionary<ComponentTypeId, Type> _componentTypes = new();
    
    private readonly Queue<ComponentTypeId> _availableComponentIds = new();

    public ComponentManager()
    {
        for (ComponentTypeId id = 0; id < EcsConstants.MaxComponents; id++)
        {
            _availableComponentIds.Enqueue(id);
        }
    }
    
    public void RegisterComponentType<TComponent>()
    {
        RegisterComponentType(typeof(TComponent));
    }
    
    public void RegisterComponentType(Type componentType)
    {
        if (!typeof(IComponent).IsAssignableFrom(componentType))
        {
            Logger.Error(this, 
                $"Type {componentType.Name} must implement {nameof(IComponent)} to be registered as a component");
            return;
        }

        if (_componentTypeIds.ContainsKey(componentType))
        {
            Logger.Error(this, $"Type {componentType.Name} is already registered");
            return;
        }
        
        ComponentTypeId id = _availableComponentIds.Dequeue();
        Logger.Log(this, $"Registering component type {componentType.Name}, identifying as {id}");
        _componentTypeIds[componentType] = id;
        _componentTypes[id] = componentType;
    }

    public List<Type> DeconstructSignature(Signature signature)
    {
        List<Type> result = new();
        foreach (var componentType in _componentTypes.Values)
        {
            if (signature.IsSet((int) _componentTypeIds[componentType])) 
                result.Add(componentType); 
        }
        return result;
    }

    public ComponentTypeId GetTypeIdByType(Type componentType) => _componentTypeIds[componentType];

    public Type GetTypeByTypeId(ComponentTypeId componentTypeId) => _componentTypes[componentTypeId];
}