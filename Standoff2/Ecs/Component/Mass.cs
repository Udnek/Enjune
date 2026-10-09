using Enjune.Ecs.Component;
using Enjune.Registering;

namespace Standoff2.Ecs.Component;

public record struct Mass(double Value) : IComponent
{
    public Identifier Id()
    {
        throw new NotImplementedException();
    }
}