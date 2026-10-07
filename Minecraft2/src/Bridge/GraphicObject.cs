using Enjune.Graphic.Modeling.Utility;

namespace Minecraft2.Bridge;

public struct GraphicObject
{
    public Matrix4 TransformMatrix;
    public required IRenderableModel Model;
}