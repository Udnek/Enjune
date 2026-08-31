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

public sealed class Column<T>(int capacity = EcsConstants.InitialColumnCapacity) : IColumn where T : struct, IComponent
{
    private T[] _data = new T[capacity];
    internal ref T this[int i] => ref _data[i];
    public int Count { get; set; }

    public void SetValue(int row, IComponent value) => _data[row] = (T)value;

    public void SwapElements(int rowFrom, int rowTo) => _data[rowFrom] = _data[rowTo];

    public IComponent GetValue(int row) => _data[row];

    public Span<T> GetSpan() => _data.AsSpan(0, Count);

    void IColumn.SetCapacity(int capacity) => Array.Resize(ref _data, capacity);
}