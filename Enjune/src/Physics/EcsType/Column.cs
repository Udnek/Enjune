using System.Runtime.CompilerServices;

namespace Enjune.Physics.EcsType;

public interface IColumn
{
    void SetCapacity(int capacity);
    void SwapElements(int rowFrom, int rowTo);
}

public class Column<T> : IColumn where T : struct
{
    public T[] Data;

    public Column(int capacity = EcsConstants.InitialCapacity)
    {
        Data = new T[capacity];
    }

    public void SetValue(ref T value, int row)
    {
        Data[row] = value;
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