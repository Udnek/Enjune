namespace Enjune.Graphic.GraphicApi;

public class WhiteVertexBuffer(int elementsCapacity) : VertexBuffer<WhiteVertexData>(elementsCapacity)
{
    public void PutMesh(Mesh mesh)
    {
        var offset = Vbo.Count / UncoloredVertexSize;
        // foreach (var meshIndex in mesh.Indexes)
        // {
        //     Ebo.Put(offset + meshIndex);
        // }
        //
        // for (int i = 0; i < mesh.Vertices.Length; i++)
        // {
        //     VboTexLayers.Put(mesh.TextureId);
        // }
        //
        // for (var i = 0; i < mesh.Vertices.Length; i++)
        // {
        //     var vertex = mesh.Vertices[i];
        //     VboMain.Put(vertex.X);
        //     VboMain.Put(vertex.Y);
        //     VboMain.Put(vertex.Z);
        //
        //     VboMain.Put(mesh.Textures[i].X);
        //     VboMain.Put(mesh.Textures[i].Y);
        // }
    }

    public override bool ProvidesColor() => false;
}