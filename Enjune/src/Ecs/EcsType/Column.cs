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

/// <summary>
/// Stores <typeparamref name="TComponent"/> instances in a dense array.<br/>
/// Allows for array indexer access, returning <c>ref <typeparamref name="TComponent"/></c>.
/// </summary>
/// <typeparam name="TComponent"></typeparam>
/// <param name="capacity"></param>
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

    /// <summary>
    /// Returns a copy of the value at <paramref name="row"/>
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public IComponent GetValue(int row) => _data[row];

    void IColumn.SetCapacity(int capacity) => Array.Resize(ref _data, capacity);
}