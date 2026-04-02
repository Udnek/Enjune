namespace Enjune.Graphic.GraphicApi;

public abstract class VertexBuffer<T> where T : unmanaged
{
    public readonly FixedBuffer<T> Vbo;
    public readonly FixedBuffer<int> Ebo;
    public readonly bool ProvidesColor;
    
    public VertexBuffer(bool providesColor, int elementsCapacity)
    {
        ProvidesColor = providesColor;
        var eboCap = (int) Math.Ceiling(elementsCapacity * 6.0 / 4.0); // approximate calcs: each quad has 4 vertices and 6 indexes
        Vbo = new FixedBuffer<T>(elementsCapacity);
        Ebo = new FixedBuffer<int>(eboCap);
    }
    
    public void Clear()
    {
        Vbo.Clear();
        Ebo.Clear();
    }
}