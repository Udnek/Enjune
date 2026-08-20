using Enjune.Ecs.EcsType;
using Enjune.Graphic.Api;

namespace SceneMaker.Bridge;

public class GraphicBridge
{
    public readonly Dictionary<Entity, GraphicObject> Objects = [];
    public readonly Dictionary<Entity, SpotLight> SpotLights = [];
}