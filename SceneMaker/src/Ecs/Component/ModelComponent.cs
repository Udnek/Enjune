using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Graphic.Modeling;
using Enjune.Registering;

namespace SceneMaker.Ecs.Component;

public record struct ModelComponent() : IComponent
{
    public static readonly ICodec<ModelComponent> Codec = Codecs
            .ForEmptyConstructor(() => new ModelComponent())
            .ForField("model_reference", i => i.Model, (ref i, v) => i.Model = v, RegistryReference<Model>.Codec)
            .ForField("drops_shadow", i => i.DropsShadow, (ref i, v) => i.DropsShadow = v, Codecs.Boolean)
            .ForField("is_hidden", i => i.IsHidden, (ref i, v) => i.IsHidden = v, Codecs.Boolean).Build();

    public RegistryReference<Model> Model;
    public bool DropsShadow = true;
    public bool IsHidden = false;
    public Guid GraphicId = Guid.NewGuid(); // no need to serialize
    
    public ModelComponent(RegistryReference<Model> Model) : this() => this.Model = Model;

    public Identifier Id() => Identifier.Of(Program.Assembly, "model");
}