using Enjune.Attribute;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Misc;

namespace Enjune.Ecs.Manager;

[LogParams(logCallingMethod: true)]
public sealed class ComponentManager
{
    private readonly Dictionary<Type, ComponentTypeId> _typeToId = new();
    private readonly Dictionary<ComponentTypeId, Type> _idToType = new();
    
    private readonly Queue<ComponentTypeId> _availableComponentIds = new();

    public ComponentManager()
    {
        for (ComponentTypeId id = 0; id < EcsConstants.MaxComponentsPerEntity; id++)
        {
            _availableComponentIds.Enqueue(id);
        }
    }

    public void RegisterComponentType(Type componentType)
    {
        if (!typeof(IComponent).IsAssignableFrom(componentType))
        {
            Logger.Error(this, 
                $"Type {componentType.Name} must implement {nameof(IComponent)} to be registered as a component");
            return;
        }

        if (_typeToId.ContainsKey(componentType))
        {
            Logger.Error(this, $"Type {componentType.Name} is already registered");
            return;
        }
        
        ComponentTypeId id = _availableComponentIds.Dequeue();
        Logger.Info(this, $"Registering component type {componentType.Name}, identifying as {id}");
        _typeToId[componentType] = id;
        _idToType[id] = componentType;
    }

    public List<Type> DeconstructSignature(Signature signature)
    {
        List<Type> result = new();
        foreach (var componentType in _idToType.Values)
        {
            if (signature.IsSet((int) _typeToId[componentType])) 
                result.Add(componentType); 
        }
        return result;
    }

    public ComponentTypeId GetIdByType(Type componentType)
    {
        if (!_typeToId.ContainsKey(componentType))
        {
            Logger.Error(this, $"Unknown component {componentType} requested. Did you forget to register it?");
            throw new KeyNotFoundException($"Component type {componentType} was not registered in {this}");
        }
        return _typeToId[componentType];
    }

    public Type GetTypeByTypeId(ComponentTypeId componentTypeId) => _idToType[componentTypeId];
}