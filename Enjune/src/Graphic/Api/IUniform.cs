namespace Enjune.Graphic.Api;

public interface IUniform<T> where T : unmanaged
{
    public void Set(T value);
}