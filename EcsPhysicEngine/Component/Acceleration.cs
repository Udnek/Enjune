using Enjune;
using Enjune.Ecs.Component;
using Enjune.Registering;
using System.Reflection;

namespace PhysicsLibrary.Component;

public record struct Acceleration(
    double X,
    double Y,
    double Z
) : IComponent
{
    public Identifier Id()
    {
        return Identifier.Of(Enjune.Enjune.Assembly, "acceleration");
    }

    public override string ToString() => $"({X}, {Y}, {Z})";
}
