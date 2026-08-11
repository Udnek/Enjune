using Enjune.Ecs.Component;

namespace SceneMaker.Ecs.Component;

public record struct Acceleration(
    double X,
    double Y,
    double Z
) : IComponent
{
    public override string ToString() => $"({X}, {Y}, {Z})";
}
