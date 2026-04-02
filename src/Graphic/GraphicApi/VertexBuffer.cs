namespace Enjune.Graphic.GraphicApi;

public abstract class VertexBuffer<T> where T : unmanaged
{
    public const int ColoredVertexSize = 3 + 4 + 2; // pos + color + tex
    public const int UncoloredVertexSize = 3 + 2; // pos + tex
    
    public readonly FixedBuffer<T> Vbo;
    public readonly FixedBuffer<int> Ebo;
    
    public VertexBuffer(int elementsCapacity)
    {
        var eboCap = (int) Math.Ceiling(elementsCapacity * 6.0 / 4.0); // approximate calcs: each quad has 4 vertices and 6 indexes
        Vbo = new FixedBuffer<T>(elementsCapacity);
        Ebo = new FixedBuffer<int>(eboCap);
    }

    public abstract bool ProvidesColor();
    
    public void Clear()
    {
        Vbo.Clear();
        Ebo.Clear();
    }
}