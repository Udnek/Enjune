using System.ComponentModel;
using System.Diagnostics;
using Enjune.Physics.EcsType;

namespace Enjune.Physics.Manager;
using System;

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
    
    public void RegisterComponentType(Type componentType)
    {
        ComponentTypeId id = _availableComponentIds.Dequeue();
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

    public Signature ConstructSignature(List<ComponentTypeId> componentTypeIds)
    {
        var signature = new Signature(0);
        foreach (ComponentTypeId componentId in componentTypeIds)
        {
            signature.Set((int) componentId);
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