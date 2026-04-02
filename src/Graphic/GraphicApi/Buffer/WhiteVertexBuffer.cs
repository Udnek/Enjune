using Enjune.Graphic.GraphicApi.Data;

namespace Enjune.Graphic.GraphicApi.Buffer;

public class WhiteVertexBuffer(int elementsCapacity) : VertexBuffer<WhiteVertexData>(false, elementsCapacity)
{
    public void PutMesh(Mesh mesh)
    {
        var offset = Vbo.Count;
        foreach (var meshIndex in mesh.Indexes)
        {
            Ebo.Put(offset + meshIndex);
        }
        for (var i = 0; i < mesh.Vertices.Length; i++)
        {
            Vbo.Put(new WhiteVertexData(mesh.Vertices[i], mesh.Textures[i], mesh.TextureId));
        }
    }
}