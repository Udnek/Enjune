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
    public T[] Data;

    public Column(int capacity = EcsConstants.InitialCapacity)
    {
        Data = new T[capacity];
    }

    public void SetValue(int row, IComponent value)
    {
        Data[row] = (T)value;
    }
    
    public void SetRawData(byte[] rawData, int row)
    {
        Data[row] = Unsafe.ReadUnaligned<T>(ref rawData[0]);
    }

    public void SwapElements(int rowFrom, int rowTo)
    {
        Data[rowFrom] = Data[rowTo];
    }

    public IComponent GetValue(int row)
    {
        return Data[row];
    }

    public Span<T> GetSpan()
    {
        return Data.AsSpan();
    }

    void IColumn.SetCapacity(int capacity)
    {
        Array.Resize(ref Data, capacity);
    }
}