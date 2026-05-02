using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;
using OpenGLApi.Component.Uniform;

namespace OpenGLApi.Shader;

public sealed class ScreenShader : AbstractShader
{
    private readonly Fbo _fbo;
    private readonly Vao _vao;
    private readonly Vbo<(Vector2 position, TextureCoord texCoord)> _vbo;
    private TextureUniform _texture = null!;
    private readonly int _textureUnitId;

    public ScreenShader(Vao vao, Vbo<(Vector2 position, TextureCoord texCoord)> vbo, EmptyTexture texture)
    {
        _vao = vao;
        _vbo = vbo;
        _textureUnitId = texture.GetUnitId();
    }

    protected override void InitUniforms()
    {
        _texture = new TextureUniform("uScreenTexture", _textureUnitId, this);
    }

    public override void AfterBind()
    {
        Fbo.BindDefault();
        GL.Disable(EnableCap.DepthTest);
    }

    public override void BeforeUnbind()
    {
        base.BeforeUnbind();
        GL.Enable(EnableCap.DepthTest);
    }

    public void Render()
    {
        _vao.Bind();
        _vbo.Bind();
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
}