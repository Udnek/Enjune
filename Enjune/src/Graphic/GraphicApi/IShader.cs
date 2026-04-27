using Enjune.Graphic.GraphicApi.Vertex.Colored;
using Enjune.Graphic.GraphicApi.Vertex.Material;

namespace Enjune.Graphic.GraphicApi;

public interface IShader
{
    void ModelTransform(Matrix4 model);
    void ViewTransform(Matrix4 view);
    void ProjectionTransform(Matrix4 proj);
    void GlobalColor(Color color);
    
    public interface IMaterial : IShader
    {
        void ViewPosition(Position position);
        void RenderToScreenBuffer(MaterialVertexBuffer buffer);
    }
    
    public interface IColor: IShader
    {
        void RenderToScreenBuffer(ColoredVertexBuffer buffer, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle);
    }
}