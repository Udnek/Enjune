using Enjune.Graphic.GraphicApi.Data;

namespace Enjune.Graphic.GraphicApi;

public abstract class VertexBuffer
{
    public readonly FixedBuffer<VertexData> VertexVbo;
    public readonly FixedBuffer<MatId> MaterialIdVbo;
    public readonly FixedBuffer<int> Ebo;
    
    public VertexBuffer(int vertexCapacity)
    {
        VertexVbo = new FixedBuffer<VertexData>(vertexCapacity);
        var eboCap = (int) Math.Ceiling(vertexCapacity * 6.0 / 4.0); // approximate calcs: each quad has 4 vertices and 6 indexes
        Ebo = new FixedBuffer<int>(eboCap);
        var matVboCap = eboCap / 3; // 1 material per 3 vertexes
        MaterialIdVbo = new FixedBuffer<MatId>(matVboCap);
    }
    
    public void PutMesh(Mesh mesh)
    {
        // ebo
        var eboOffset = VertexVbo.Count;
        foreach (var meshIndex in mesh.Indexes)
        {
            Ebo.Put(eboOffset + meshIndex);
        }
        // vertices
        for (var i = 0; i < mesh.Vertices.Length; i++)
        {
            VertexVbo.Put(new VertexData(mesh.Vertices[i], mesh.Textures[i]));
        }
        // materials
        for (var _ = 0; _ < mesh.Indexes.Length / 3; _++)
        {
            MaterialIdVbo.Put(mesh.MatId);
        }
    }

    public void Clear()
    {
        VertexVbo.Clear();
        MaterialIdVbo.Clear();
        Ebo.Clear();
    }
}