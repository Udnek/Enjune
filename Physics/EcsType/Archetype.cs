using System.ComponentModel;
using System.Globalization;
using Enjune.Physics.EcsType;
using IComponent = Enjune.Physics.Component.IComponent;

namespace Enjune.Physics.EcsType;

public class Archetype
{
    private readonly Signature _signature;
    
    private Dictionary<Type, List<IComponent>> _components = new();
    public Dictionary<EntityId, int> Id2Row = new();
    public Dictionary<int, EntityId> Row2Id = new();
    
    private int _lastRow;
    public int Count;

    public Archetype(Signature signature)
    {
        _signature = signature;
        int nComponents = signature.GetSetBitsCount();
        var types = World.ComponentManager.DeconstructSignature(_signature);
        foreach (var type in types)
        {
            _components.Add(type, new List<IComponent>());
        }
    }

    public bool AssertSignature(Signature otherSignature) => _signature == otherSignature;
    
    public void AddEntity(EntityId id, List<IComponent> components)
    {
        int index = _lastRow;
        _lastRow++;
        Id2Row.Add(id, index);
        Row2Id.Add(index, id);
        foreach (var component in components) { _components[component.GetType()].Add(component); }
    }

    public void RemoveEntity(EntityId id)
    {
        int index = Id2Row[id];
        _lastRow--;
        Id2Row.Remove(id);
        Row2Id.Remove(id);
        foreach (var componentList in _components.Values)
        {
            componentList[index] = componentList.Last();
            componentList.RemoveAt(componentList.Count - 1);
        }
    }
}