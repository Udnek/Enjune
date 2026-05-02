using Enjune.Graphic.GraphicApi;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Uniform;

namespace OpenGLApi.Shader;

public abstract class CameraShader(Fbo fbo) : AbstractShader, IShader.ICamera
{
    private Matrix4Uniform _model = null!;
    private Matrix4Uniform _view = null!;
    private Matrix4Uniform _projection = null!;
    private Vector4Uniform _globalColor = null!;
    private readonly Fbo _fbo = fbo;

    protected override void InitUniforms()
    {
        _model = new Matrix4Uniform("uModel", Matrix4.Identity, this);
        _view = new Matrix4Uniform("uView", Matrix4.Identity, this);
        _projection = new Matrix4Uniform("uProjection", 
            Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1.0f, 0.1f, 1000.0f), 
            this);
        _globalColor = new Vector4Uniform("uGlobalColor", Color.One, this);
    }

    public void ModelTransform(Matrix4 model) => _model.SetValue(model);
    public void ProjectionTransform(Matrix4 proj) => _projection.SetValue(proj);
    public void ViewTransform(Matrix4 view) => _view.SetValue(view);
    public void GlobalColor(Color color) => _globalColor.SetValue(color);

    public override void AfterBind() => _fbo.Bind();
}