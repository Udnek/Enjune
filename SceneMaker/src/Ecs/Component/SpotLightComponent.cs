using Enjune.Data.Codec;
using Enjune.Ecs;
using Enjune.Ecs.Component;
using Enjune.Graphic.Api;

namespace SceneMaker.Ecs.Component;

public class SpotLightComponent(SpotLight value) : IComponent
{
    public static readonly ICodec<SpotLightComponent> Codec = Codecs.ForOneArgConstructor(
        v => new SpotLightComponent(v), i => i.Value, SpotLight.Codec);
    
    
    static SpotLightComponent() => ComponentCodecRegistry.Register("spot_light", Codec);

    public SpotLight Value = value;
}