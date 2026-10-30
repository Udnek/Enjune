using Enjune;
using Enjune.Ecs.Component;
using Enjune.Registering;
using System.Numerics;
using System.Reflection;

namespace PhysicsLibrary.General.Component;

public record struct Transform : IComponent
{
    public Identifier Id() => Identifier.Of(Enjune.Enjune.Assembly, "transform");

    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
}
