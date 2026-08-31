using Enjune.Graphic.Api;

namespace SceneMaker.Bridge;

public struct GraphicObject
{
    public Matrix4 TransformMatrix;
    public IRenderableModel Model;
    public bool IsHidden = false;
    public bool DropsShadow = true;
    public bool IsHighlighted = false;

    public GraphicObject(IRenderableModel model) => Model = model;
}