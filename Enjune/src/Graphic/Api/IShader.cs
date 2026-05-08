namespace Enjune.Graphic.Api;

public interface IShader
{
    
    public interface IShadowMap : IShader
    {
        void ForEachLight(Action action);
        void ModelTransform(Matrix4 model);
    }
    
    public interface ICamera : IShader
    {
        void ModelTransform(Matrix4 model);
        void ViewTransform(Matrix4 view);
        void ProjectionTransform(Matrix4 proj);
        void GlobalColor(Color color);
        
        public interface IMaterial : ICamera
        {
            void ViewPosition(Position position);
        }
    
        public interface IColor: ICamera;
    }
}