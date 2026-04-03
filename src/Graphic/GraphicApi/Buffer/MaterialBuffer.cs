using Enjune.Graphic.GraphicApi.Data;

namespace Enjune.Graphic.GraphicApi;

public abstract class MaterialBuffer
{
    public readonly FixedBuffer<MaterialData> Vbo;
    
    public MaterialBuffer(int capacity)
    {
        Vbo = new FixedBuffer<MaterialData>(capacity);
    }
    
    public void Put(MaterialData materialData)
    {
        Vbo.Put(materialData);
    }

    public void Clear() => Vbo.Clear();
}