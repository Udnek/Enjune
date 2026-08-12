using System.Runtime.CompilerServices;
using Enjune.Ecs.Component;

namespace Enjune.Ecs.EcsType;

public interface IColumn
{
    void SetValue(int row, IComponent value);
    void SetCapacity(int capacity);
    void SwapElements(int rowFrom, int rowTo);
    IComponent GetValue(int row);
}

public sealed class Column<T> : IColumn where T : struct, IComponent
{
    private T[] _data;

    public Column(int capacity = EcsConstants.InitialCapacity) => _data = new T[capacity];

    public void SetValue(int row, IComponent value) => _data[row] = (T)value;

    public void SwapElements(int rowFrom, int rowTo) => _data[rowFrom] = _data[rowTo];

    public IComponent GetValue(int row) => _data[row];

    public Span<T> GetSpan() => _data.AsSpan();

    void IColumn.SetCapacity(int capacity) => Array.Resize(ref _data, capacity);
}