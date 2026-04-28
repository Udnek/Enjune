using Enjune.Graphic.GraphicApi.Vertex.Colored;
using Enjune.Graphic.GraphicApi.Vertex.Material;

namespace Enjune.Graphic.GraphicApi;

public interface IShader
{
    public interface I3D : IShader
    {
        void ModelTransform(Matrix4 model);
        void ViewTransform(Matrix4 view);
        void ProjectionTransform(Matrix4 proj);
        void GlobalColor(Color color);
        
        public interface IMaterial : I3D
        {
            void Lights(IEnumerable<PointLight> lights);
            void ViewPosition(Position position);
            void Render(MaterialVertexBuffer buffer);
        }
    
        public interface IColor: I3D
        {
            void Render(ColoredVertexBuffer buffer, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle);
        }
    }
    

}