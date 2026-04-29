using Enjune.Graphic.GraphicApi;
using OpenGLApi.Component;
using OpenGLApi.Component.Uniform;

namespace OpenGLApi.Shader;

public abstract class Shader3D : AbstractShader, IShader.I3D
{
    private Matrix4Uniform _model = null!;
    private Matrix4Uniform _view = null!;
    private Matrix4Uniform _projection = null!;
    private Vector4Uniform _globalColor = null!;
    
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
    
    protected static PrimitiveType PrimitiveFromApi(IGraphicApi.Primitive primitive)
    {
        return primitive switch
        {
            IGraphicApi.Primitive.Triangle => PrimitiveType.Triangles,
            IGraphicApi.Primitive.LineStrip => PrimitiveType.LineStrip,
            IGraphicApi.Primitive.LineLoop => PrimitiveType.LineLoop,
            IGraphicApi.Primitive.Line => PrimitiveType.Lines,
            IGraphicApi.Primitive.Point => PrimitiveType.Points,
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null)
        };
    }
}