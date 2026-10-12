namespace Enjune.Graphic.Api;

public interface IUniformArray<T> where T : unmanaged
{
    public int Count { get; }
    public int Capacity { get; }
    public void Push(ReadOnlySpan<T> data);
    public T this[int p] { set; }
}