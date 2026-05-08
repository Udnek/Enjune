using Enjune.Graphic.Api;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;

namespace OpenGLApi.Shader;

public abstract class AbstractShader : ShaderProgram, IShader
{
    public abstract void AfterBind();

    public virtual void BeforeUnbind() => Fbo.BindDefault();
}