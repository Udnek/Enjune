using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Registering;

namespace SceneMaker.Ecs.Component;

public record struct SpotLightComponent() : IComponent
{
    public static readonly ICodec<SpotLightComponent> Codec = Codecs
        .ForEmptyConstructor(() => new SpotLightComponent())
        .ForField("projection", i => i.Projection, (ref i, v) => i.Projection = v, Codecs.Matrix4)
        .ForField("color", i => i.Color, (ref i, v) => i.Color = v, Codecs.Vector4).Build();
    
    public Matrix4 Projection;
    public Color Color;
    public Guid GraphicId = Guid.NewGuid(); // no need to serialize
    
    //public SpotLightComponent(){}

    public Identifier Id() => Identifier.Of(Program.Assembly, "spot_light");
}