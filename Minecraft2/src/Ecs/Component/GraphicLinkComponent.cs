using Enjune.Ecs.Component;
using Enjune.Graphic.Modeling;
using Enjune.Registering;

namespace Minecraft2.Ecs.Component;

public struct GraphicLinkComponent() : IComponent
{
    public Guid GraphicId = Guid.NewGuid();
    public required ChunkModel Model;

    public Identifier Id() => Identifier.Of(Program.Assembly, "graphic_link");
}