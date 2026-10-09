using Enjune.Ecs.Component;
using Enjune.Registering;

namespace Standoff2.Ecs.Component;

public record struct Position(double X, double Y, double Z) : IComponent
{
    public Identifier Id()
    {
        throw new NotImplementedException();
    }

    public override string ToString() => "(" + X + ", " + Y + ", " + Z + ")";
}
