using Enjune.Data.Codec;
using Enjune.Ecs;
using Enjune.Ecs.Component;
using Enjune.Graphic.Api;
using Enjune.Registering;

namespace SceneMaker.Ecs.Component;

public record class SpotLightComponent(SpotLight Value) : IComponent
{
    public static readonly ICodec<SpotLightComponent> Codec = Codecs.ForOneArgConstructor(
        v => new SpotLightComponent(v), i => i.Value, SpotLight.Codec);

    public Identifier Id() => Identifier.Of(Program.Assembly, "spot_light");
}