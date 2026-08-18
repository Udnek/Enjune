using Enjune.Graphic.Api;

namespace SceneMaker.Bridge;

public struct GraphicObject
{
    public Matrix4 TransformMatrix;
    public IRenderableModel Model;
    public bool Hidden = false;
    public bool DropsShadow = true;

    public GraphicObject(IRenderableModel model) => Model = model;
}