namespace Enjune.Graphic.GraphicApi;

public class WhiteVertexBuffer(int verticesCapacity) : VertexBuffer(UncoloredVertexSize, verticesCapacity)
{
    public void PutMesh(Mesh mesh)
    {
        var offset = Vbo.Count / UncoloredVertexSize;
        foreach (var meshIndex in mesh.Indexes)
        {
            Ebo.Put(offset + meshIndex);
        }

        for (var i = 0; i < mesh.Vertices.Length; i++)
        {
            var vertex = mesh.Vertices[i];
            Vbo.Put(vertex.X);
            Vbo.Put(vertex.Y);
            Vbo.Put(vertex.Z);

            Vbo.Put(mesh.Textures[i].X);
            Vbo.Put(mesh.Textures[i].Y);
        }
    }

    public override bool ProvidesColor() => false;
}