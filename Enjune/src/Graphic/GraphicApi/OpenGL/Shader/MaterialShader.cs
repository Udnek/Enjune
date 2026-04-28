using Enjune.Graphic.GraphicApi.OpenGL.Component;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Buffer;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Texture;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;
using Enjune.Graphic.GraphicApi.Vertex.Material;

namespace Enjune.Graphic.GraphicApi.OpenGL.Shader;

public sealed class MaterialShader : Shader3D<MaterialVertexData>, IShader.I3D.IMaterial
{
    private Vector3Uniform _viewPos = null!;
    private IntUniform _lightsLength = null!;
    private TextureUniform _textureUniform = null!;
    
    private readonly Ssbo<MatId> _matIdSsbo;
    private readonly Ebo _ebo;
    private readonly int _textureUnitId;
    private readonly Ssbo<PointLightData> _lightSsbo;

    public MaterialShader(Vao vao, Vbo<MaterialVertexData> vbo, Ssbo<MatId> matIdSsbo, Ebo ebo, TextureArray texture, Ssbo<PointLightData> lightSsbo) 
        : base(vao, vbo)
    {
        _matIdSsbo = matIdSsbo;
        _ebo = ebo;
        _textureUnitId = texture.GetUnitId();
        _lightSsbo = lightSsbo;
    }

    protected override void InitUniforms()
    {
        base.InitUniforms();
        _viewPos = new Vector3Uniform("uViewPos", Vector3.Zero, this);
        _textureUniform = new TextureUniform("uTextures", _textureUnitId, this);
        _lightsLength = new IntUniform("uLightsLength", 0, this);
    }


    public void Lights(IEnumerable<PointLight> lights)
    {
        _lightSsbo.BindAndPush(lights.Select(l => new PointLightData(l.Position, l.Color)).ToArray());
        _lightsLength.SetValue(lights.Count());
    }

    public void ViewPosition(Position position) => _viewPos.SetValue(position);

    public void Render(MaterialVertexBuffer buffer)
    {
        Vao.Bind();
        Vbo.BindAndPush(buffer.VertexVbo);
        _matIdSsbo.BindAndPush(buffer.MatIdSsbo);
        _ebo.BindAndPush(buffer.Ebo);

        GL.DrawElements(BeginMode.Triangles, buffer.Ebo.Count, DrawElementsType.UnsignedInt, 0);
    }
}