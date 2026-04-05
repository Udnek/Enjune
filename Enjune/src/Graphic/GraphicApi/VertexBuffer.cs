namespace Enjune.Graphic.GraphicApi;

public sealed class VertexBuffer
{
    public readonly FixedBuffer<VertexData> VertexVbo;
    public readonly FixedBuffer<MatId> MatIdSsbo;
    public readonly FixedBuffer<int> Ebo;
    
    public VertexBuffer(int vertexCapacity)
    {
        VertexVbo = new FixedBuffer<VertexData>(vertexCapacity);
        var eboCap = (int) Math.Ceiling(vertexCapacity * 6.0 / 4.0); // approximate calcs: each quad has 4 vertices and 6 indexes
        Ebo = new FixedBuffer<int>(eboCap);
        var matVboCap = eboCap / 3; // 1 material per 3 vertexes
        MatIdSsbo = new FixedBuffer<MatId>(matVboCap);
    }

    public void PutModel(Model model)
    {
        foreach (var (mesh, material) in model.Meshes)
            PutMesh(mesh, material.Id);
    }
    
    public void PutMesh(Mesh mesh, MatId matId)
    {
        // ebo
        var eboOffset = VertexVbo.Count;
        foreach (var meshIndex in mesh.Indexes) 
            Ebo.Put(eboOffset + meshIndex);
        
        // vertices
        for (var i = 0; i < mesh.Vertices.Length; i++)
            VertexVbo.Put(new VertexData(mesh.Vertices[i], mesh.Textures[i]));
        
        // materials
        for (var _ = 0; _ < mesh.Indexes.Length / 3; _++)
            MatIdSsbo.Put(matId);
    }

    public void Clear()
    {
        VertexVbo.Clear();
        MatIdSsbo.Clear();
        Ebo.Clear();
    }
}