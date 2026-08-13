using Enjune.Graphic.Api;

namespace SceneMaker.Bridge;

public class GraphicBridge
{
    public readonly Dictionary<EntityId, GraphicObject> Objects = [];
    public readonly Dictionary<EntityId, SpotLight> SpotLights = [];
}