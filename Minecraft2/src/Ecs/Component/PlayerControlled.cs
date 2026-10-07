using Enjune.Ecs.Component;
using Enjune.Registering;

namespace Minecraft2.Ecs.Component;

public struct PlayerControlled : IComponent
{
    public Identifier Id() => Identifier.Of(Program.Assembly, "player_controlled");
}