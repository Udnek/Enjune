namespace Enjune.Physics;

public class SparseSet<T>
{
    private T[] _dense;
    private int[] _sparse;
    private int _nElements;
    private int _maxValue;
    private int _capacity;

    public SparseSet(int maxVal, int capacity)
    {
        _sparse = new int[maxVal+1];
        _dense = new T[capacity];
        _capacity = capacity;
        _maxValue = maxVal;
        _nElements = 0;
    }

}