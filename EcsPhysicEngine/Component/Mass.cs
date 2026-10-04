using Enjune.Ecs.Component;
using Enjune.Registering;

namespace PhysicsLibrary.Component;

public record struct Mass(double Value) : IComponent
{
    public Identifier Id()
    {
        return Identifier.Of(Enjune.Enjune.Assembly, "mass");
    }
}