using Enjune.Ecs.Component;
using Enjune.Registering;

namespace Minecraft2.Ecs.Component;

public struct ToBeRemovedRequest : IComponent
{
    public Identifier Id() => Identifier.Of(Program.Assembly, "to_be_removed_request");
}