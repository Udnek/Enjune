using Enjune.Ecs.Component;
using Enjune.Registering;

namespace PhysicsLibrary.Component;

public record struct Position(double X, double Y, double Z) : IComponent
{
    public Identifier Id()
    {
        return Identifier.Of(Enjune.Enjune.Assembly, $"position");
    }

    public override string ToString() => "(" + X + ", " + Y + ", " + Z + ")";
}
