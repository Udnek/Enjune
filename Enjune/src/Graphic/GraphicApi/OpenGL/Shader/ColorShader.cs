using Enjune.Graphic.GraphicApi.OpenGL.Component;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Buffer;
using Enjune.Graphic.GraphicApi.Vertex.Colored;

namespace Enjune.Graphic.GraphicApi.OpenGL.Shader;

public sealed class ColorShader : Shader3D<ColoredVertexData>, IShader.I3D.IColor
{
    private readonly Ebo _ebo;

    public ColorShader(Vao vao, Vbo<ColoredVertexData> vbo, Ebo ebo) : base(vao, vbo)
    {
        _ebo = ebo;
    }

    public void Render(ColoredVertexBuffer buffer, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle)
    {
        Vao.Bind();
        Vbo.BindAndPush(buffer.Vbo);
        _ebo.BindAndPush(buffer.Ebo);
        GL.DrawElements(PrimitiveFromApi(primitive), buffer.Ebo.Count, DrawElementsType.UnsignedInt, 0);
    }
}