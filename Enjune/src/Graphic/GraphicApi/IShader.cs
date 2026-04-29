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
        }
    
        public interface IColor: I3D;
    }
}