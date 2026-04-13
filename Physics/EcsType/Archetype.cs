using System.ComponentModel;
using System.Globalization;
using Enjune.Physics.EcsType;
using IComponent = Enjune.Physics.Component.IComponent;

namespace Enjune.Physics.EcsType;

public class Archetype
{
    private readonly Signature _signature;
    
    private Array[] _componentArrays;
    public Dictionary<EntityId, int> Id2Row = new();
    public Dictionary<int, EntityId> Row2Id = new();

    private int _entityCapacity = 32;
    private int _lastRow;
    public int Count;

    public Archetype(Signature signature)
    {
        _signature = signature;
        int nComponents = signature.GetSetBitsCount();
        List<Type> types = World.ComponentManager.DeconstructSignature(_signature);
        _componentArrays = new Array[nComponents];
        for (int i = nComponents - 1; i >= 0; i--)
        {
            _componentArrays[i] = Array.CreateInstance(types[i], _entityCapacity);
        }
    }

    public bool AssertSignature(Signature otherSignature) => _signature == otherSignature;
    
    public void AddEntity(EntityId id, List<IComponent> components)
    {
        int index = _lastRow;
        _lastRow++;
        Id2Row.Add(id, index);
        Row2Id.Add(index, id);
        //foreach (var component in components) { _components[component.GetType()].Add(component); }
    }

    public void RemoveEntity(EntityId id)
    {
        int index = Id2Row[id];
        _lastRow--;
        Id2Row.Remove(id);
        Row2Id.Remove(id);
        /*foreach (var componentList in _components.Values)
        {
            componentList[index] = componentList.Last();
            componentList.RemoveAt(componentList.Count - 1);
        }*/
    }
}