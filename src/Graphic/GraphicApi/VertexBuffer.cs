namespace Enjune.Graphic.GraphicApi;

public abstract class VertexBuffer
{
    public const int ColoredVertexSize = 3 + 4 + 2; // pos + color + tex
    public const int UncoloredVertexSize = 3 + 2; // pos + tex
    
    public readonly FixedBuffer<float> VboMain;
    public readonly FixedBuffer<TexId> VboTexLayers;
    public readonly FixedBuffer<int> Ebo;
    
    public VertexBuffer(int oneElementSize, int elementsCapacity)
    {
        var vboCap = oneElementSize * elementsCapacity;
        // approximate calcs: each quad has 4 vertices and 6 indexes
        var eboCap = (int) Math.Ceiling(elementsCapacity * 6.0 / 4.0);
        VboMain = new FixedBuffer<float>(vboCap);
        VboTexLayers = new FixedBuffer<int>(elementsCapacity);
        Ebo = new FixedBuffer<int>(eboCap);
    }

    public abstract bool ProvidesColor();
    
    public void Clear()
    {
        VboTexLayers.Clear();
        VboMain.Clear();
        Ebo.Clear();
    }
}