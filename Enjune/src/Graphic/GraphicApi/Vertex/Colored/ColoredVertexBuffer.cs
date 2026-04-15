namespace Enjune.Graphic.GraphicApi.Vertex.Colored;

public class ColoredVertexBuffer
{
    public readonly FixedBuffer<ColoredVertexData> Vbo;
    public readonly FixedBuffer<int> Ebo;
    
    public ColoredVertexBuffer(int capacity)
    {
        Vbo = new FixedBuffer<ColoredVertexData>(capacity);
        var eboCap = (int) Math.Ceiling(capacity * 6.0 / 4.0); // approximate calcs: each quad has 4 vertices and 6 indexes
        Ebo = new FixedBuffer<int>(eboCap);
    }


    public void PutAnyModel<T1, T2>(Model<T1, T2> model, Color color)
    {
        foreach (var (mesh, _) in model.Meshes)
            PutAnyMesh(mesh, color);
    }

    public void PutAnyMesh<T>(Mesh<T> mesh, Color color)
    {
        // ebo
        var eboOffset = Vbo.Count;
        foreach (var meshIndex in mesh.Indexes) 
            Ebo.Put(eboOffset + meshIndex);
        
        // vertices
        for (var i = 0; i < mesh.Vertices.Length; i++) 
            Vbo.Put(new ColoredVertexData(mesh.Vertices[i], color));
    }
    
    public void PutModel(Model<Color, Color> model)
    {
        foreach (var (mesh, meshColor) in model.Meshes)
            PutMesh(mesh, meshColor);
    }
    
    public void PutMesh(Mesh<Color> mesh, Color meshColor)
    {
        // ebo
        var eboOffset = Vbo.Count;
        foreach (var meshIndex in mesh.Indexes) 
            Ebo.Put(eboOffset + meshIndex);
        
        // vertices
        for (var i = 0; i < mesh.Vertices.Length; i++) 
            Vbo.Put(new ColoredVertexData(mesh.Vertices[i], mesh.PerVertexData[i] * meshColor));
    }

    public void Clear()
    {
        Vbo.Clear();
        Ebo.Clear();
    }
}