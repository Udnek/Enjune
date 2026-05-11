using System.Diagnostics;
using Enjune.Misc;
using Enjune.Physics.Component;
using Enjune.Physics.EcsType;

namespace Enjune.Physics.Manager;

public class ComponentManager
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
    
    public ComponentManager RegisterComponentType<TComponent>()
    {
        ComponentTypeId id = _availableComponentIds.Dequeue();
        Logger.Log(GetType(), $"Registering component type {typeof(TComponent).Name}, identifying as {id}");
        _componentTypeIds[typeof(TComponent)] = id;
        _componentTypes[id] = typeof(TComponent);
        return this;
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

    public Signature ConstructSignature(Span<Type> components)
    {
        var signature = new Signature(0);
        foreach (IComponent component in components)
        {
            signature.Set((int) _componentTypeIds[component.GetType()]);
        }

        return signature;
    }

    public ComponentTypeId GetTypeIdByType(Type componentType)
    {
        return _componentTypeIds[componentType];
    }

    public Type GetTypeByTypeId(ComponentTypeId componentTypeId)
    {
        return _componentTypes[componentTypeId];
    }
}