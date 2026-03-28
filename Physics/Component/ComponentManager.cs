using System.ComponentModel;

namespace Enjune.Physics;

public class Archetype
{
    public required Type[] ComponentTypes;
    public required object[] ComponentArrays;
    public required int[] Entities;
    public int Count;
}

public class ComponentManager
{
    // public ref T GetComponent<T>(Entity entity) where T : unmanaged, IComponent
    // {
    //     T result = default;
    //     return ref result;
    // }
}