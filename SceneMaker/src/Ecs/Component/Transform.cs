using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Misc;
using Enjune.Registering;

namespace SceneMaker.Ecs.Component;

public record struct Transform() : IComponent
{
    public static readonly MapCodec<Transform> Codec = Codecs
        .ForEmptyConstructor(() => new Transform())
        .ForField("position", i => i.Position, (ref i, v) => i.Position = v, Codecs.Vector3)
        .ForField("rotation", i => i.Rotation, (ref i, v) => i.Rotation = v, Codecs.Quaternion)
        .ForField("scale", i => i.Scale, (ref i, v) => i.Scale = v, Codecs.Vector3)
        .Build();
    

    public Position Position = Position.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public Matrix4 Matrix => MathUtils.CreateModelTransform(Position, Rotation, Scale);
    
    public Identifier Id() => Identifier.Of(Program.Assembly, "transform");
}