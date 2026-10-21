using Enjune.KitStart.Modeling;

namespace Minecraft2.Bridge;

public struct GraphicObject
{
    public Matrix4 TransformMatrix;
    public required DynamicRenderableModel<GraphicChunkData.Vertex, GraphicChunkData.Primitive> Model;
}