using Enjune.Graphic.Api;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Mesh;

namespace OpenGLApi.Shader;

public abstract class AbstractShader : ShaderProgram, IShader
{
    public abstract void AfterBind();

    public virtual void BeforeUnbind() => Fbo.BindDefault();
    
    public void Render(IRenderableMesh mesh) => ((GlMesh) mesh).Render();
}