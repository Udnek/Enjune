using Enjune.Graphic.GraphicApi.OpenGL.Component;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Array;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;
using Enjune.Graphic.GraphicApi.Vertex.Material;

namespace Enjune.Graphic.GraphicApi.OpenGL.Shader;

public sealed class MaterialShader : AbstractShader, IShader.IMaterial
{
    private Vector3Uniform _viewPos = null!;
    private IntUniform _lightsLength = null!;
    private TextureUniform _textureUniform = null!;
    
    private readonly Vao _vao;
    private readonly Vbo<MaterialVertexData> _vertexVbo;
    private readonly Ssbo<MatId> _matIdSsbo;
    private readonly Ebo _ebo;
    private readonly int _textureSampler;
    private readonly Ssbo<PointLightData> _lightSsbo;

    public MaterialShader(Vao vao, Vbo<MaterialVertexData> vertexVbo, Ssbo<MatId> matIdSsbo, Ebo ebo, int textureSampler, Ssbo<PointLightData> lightSsbo)
    {
        _vao = vao;
        _vertexVbo = vertexVbo;
        _matIdSsbo = matIdSsbo;
        _ebo = ebo;
        _textureSampler = textureSampler;
        _lightSsbo = lightSsbo;
    }
    
    protected override VaoAttributes CreateEmptyAttributes() => new(_vao, _vertexVbo);

    protected override void InitUniforms()
    {
        base.InitUniforms();
        _viewPos = new Vector3Uniform("uViewPos", Vector3.Zero, this);
        _textureUniform = new TextureUniform("uTextures", _textureSampler, this);
        _lightsLength = new IntUniform("uLightsLength", 0, this);
    }


    public void Lights(IEnumerable<PointLight> lights)
    {
        _lightSsbo.BindAndPush(lights.Select(l => new PointLightData(l.Position, l.Color)).ToArray());
        _lightsLength.SetValue(lights.Count());
    }

    public void ViewPosition(Position position) => _viewPos.SetValue(position);

    public void RenderToScreenBuffer(MaterialVertexBuffer buffer)
    {
        _vao.Bind();
        _vertexVbo.BindAndPush(buffer.VertexVbo);
        _matIdSsbo.BindAndPush(buffer.MatIdSsbo);
        _ebo.BindAndPush(buffer.Ebo);

        GL.DrawElements(BeginMode.Triangles, buffer.Ebo.Count, DrawElementsType.UnsignedInt, 0);
    }
}