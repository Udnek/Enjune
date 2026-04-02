using Enjune.Graphic.GraphicApi.Data;

namespace Enjune.Graphic.GraphicApi.Buffer;

public class ColoredVertexBuffer(int elementsCapacity) : VertexBuffer<ColoredVertexData>(true, elementsCapacity)
{
    public void PutMesh(Mesh mesh, Color color)
    {
        var offset = Vbo.Count;
        foreach (var meshIndex in mesh.Indexes)
        {
            Ebo.Put(offset + meshIndex);
        }
        for (var i = 0; i < mesh.Vertices.Length; i++)
        {
            Vbo.Put(new ColoredVertexData(mesh.Vertices[i], color, mesh.Textures[i], mesh.TextureId));
        }
    }
}