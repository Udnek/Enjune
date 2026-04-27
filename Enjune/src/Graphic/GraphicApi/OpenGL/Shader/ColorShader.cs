using Enjune.Graphic.GraphicApi.OpenGL.Component;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Array;
using Enjune.Graphic.GraphicApi.Vertex.Colored;

namespace Enjune.Graphic.GraphicApi.OpenGL.Shader;

public sealed class ColorShader : BaseShader, IShader.IColor
{
    private readonly Vao _vao;
    private readonly Vbo<ColoredVertexData> _vertexVbo;
    private readonly Ebo _ebo;

    public ColorShader(Vao vao, Vbo<ColoredVertexData> vertexVbo, Ebo ebo)
    {
        _vao = vao;
        _vertexVbo = vertexVbo;
        _ebo = ebo;
    }

    protected override VaoAttributes CreateEmptyAttributes() => new(_vao, _vertexVbo);

    public void RenderToScreenBuffer(ColoredVertexBuffer buffer, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle)
    {
        _vao.Bind();
        _vertexVbo.BindAndPush(buffer.Vbo);
        _ebo.BindAndPush(buffer.Ebo);
        GL.DrawElements(PrimitiveFromApi(primitive), buffer.Ebo.Count, DrawElementsType.UnsignedInt, 0);
    }
}