using System.Runtime.CompilerServices;
using Enjune.Ecs.Component;

namespace Enjune.Ecs.EcsType;

public interface IColumn
{
    int Count { get; internal set; }
    void SetValue(int row, IComponent value);
    void SetCapacity(int capacity);
    void SwapElements(int rowFrom, int rowTo);
    IComponent GetValue(int row);
}

public sealed class Column<TComponent>(int capacity = EcsConstants.InitialColumnCapacity) : IColumn where TComponent : struct, IComponent
{
    private TComponent[] _data = new TComponent[capacity];
    
    internal ref TComponent this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _data[i];
    }
    
    public int Count { get; set; }

    public void SetValue(int row, IComponent value) => _data[row] = (TComponent)value;
    
    public void SwapElements(int rowFrom, int rowTo)
    {
        var temp = _data[rowFrom];
        _data[rowFrom] = _data[rowTo];
        _data[rowTo] = temp;
    }

    public IComponent GetValue(int row) => _data[row];

    void IColumn.SetCapacity(int capacity) => Array.Resize(ref _data, capacity);
}