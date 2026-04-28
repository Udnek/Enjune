using Enjune.Graphic.GraphicApi.OpenGL.Component;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Buffer;

namespace Enjune.Graphic.GraphicApi.OpenGL.Shader;

public abstract class AbstractShader<TVbo>(Vao vao, Vbo<TVbo> vbo) : ShaderProgram, IShader
    where TVbo : unmanaged
{
    protected readonly Vao Vao = vao;
    protected readonly Vbo<TVbo> Vbo = vbo;

    protected override VaoAttributes CreateEmptyAttributes() => new(Vao, Vbo);
}