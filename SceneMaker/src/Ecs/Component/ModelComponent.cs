using Enjune.Data.Codec;
using Enjune.Ecs;
using Enjune.Ecs.Component;
using Enjune.Graphic.Api;
using Enjune.Graphic.Modeling;
using Enjune.Registering;
using SceneMaker.Misc;

namespace SceneMaker.Ecs.Component;

public struct ModelComponent() : IComponent
{
    public static readonly ICodec<ModelComponent> Codec = Codecs.ForEmptyConstructor(() => new ModelComponent())
        .ForField("model_key", i => i.ModelKey, (ref i, v) => i.ModelKey = v, Models.ResourceKeyCodec)
        .ForField("drops_shadow", i => i.DropsShadow, (ref i, v) => i.DropsShadow = v, Codecs.Boolean)
        .ForField("is_hidden", i => i.IsHidden, (ref i, v) => i.IsHidden = v, Codecs.Boolean)
        .Build();
    
    static ModelComponent() => ComponentCodecRegistry.Register("model", Codec);

    public ResourceKey<Model> ModelKey = null!;
    public bool DropsShadow = true;
    public bool IsHidden = false;
}