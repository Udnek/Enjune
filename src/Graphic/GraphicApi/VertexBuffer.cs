namespace Enjune.Graphic.GraphicApi;

public abstract class VertexBuffer
{
    public const int ColoredVertexSize = 3 + 4 + 2; // pos + color + tex
    public const int UncoloredVertexSize = 3 + 2; // pos + tex
    
    public readonly FixedBuffer<float> Vbo;
    public readonly FixedBuffer<int> Ebo;
    
    public VertexBuffer(int vertexSize, int verticesCapacity)
    {
        var vboCap = vertexSize * verticesCapacity;
        // approximate calcs: each quad has 4 vertices and 6 indexes
        var eboCap = (int) Math.Ceiling(verticesCapacity * 6.0 / 4.0);
        Vbo = new FixedBuffer<float>(vboCap);
        Ebo = new FixedBuffer<int>(eboCap);
    }

    public abstract bool ProvidesColor();
    
    public void Clear()
    {
        Vbo.Clear();
        Ebo.Clear();
    }
}