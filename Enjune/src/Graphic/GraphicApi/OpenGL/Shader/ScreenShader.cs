using Enjune.Graphic.GraphicApi.OpenGL.Component;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Buffer;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Texture;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

namespace Enjune.Graphic.GraphicApi.OpenGL.Shader;

public sealed class ScreenShader(Vao vao, Vbo<(Vector2 position, TextureCoord texCoord)> vbo, EmptyTexture texture) 
    : AbstractShader<(Vector2 position, TextureCoord texCoord)>(vao, vbo)
{
    private TextureUniform _texture = null!;
    private readonly int _textureUnitId = texture.GetUnitId();
    
    protected override void InitUniforms()
    {
        _texture = new TextureUniform("uScreenTexture", _textureUnitId, this);
    }

    public void Render()
    {
        Vao.Bind();
        Vbo.Bind();
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
}