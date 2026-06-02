using System.Runtime.CompilerServices;
using Enjune.Physics.Component;

namespace Enjune.Physics.EcsType;

public interface IColumn
{
    void SetCapacity(int capacity);
    void SwapElements(int rowFrom, int rowTo);
    void SetValue(int row, IComponent component);
}

public class Column<T> : IColumn where T : struct, IComponent
{
    public T[] Data;

    public Column(int capacity = EcsConstants.InitialCapacity)
    {
        Data = new T[capacity];
    }

    public void SetValue(int row, IComponent component)
    {
        Data[row] = (T)component;
    }
    
    public void SetRawData(byte[] rawData, int row)
    {
        Data[row] = Unsafe.ReadUnaligned<T>(ref rawData[0]);
    }

    public void SwapElements(int rowFrom, int rowTo)
    {
        Data[rowFrom] = Data[rowTo];
    }

    public T GetValue(int row)
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