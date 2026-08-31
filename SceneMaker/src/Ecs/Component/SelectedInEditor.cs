using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Registering;

namespace SceneMaker.Ecs.Component;

public struct SelectedInEditor : IComponent
{
    public static readonly ICodec<SelectedInEditor> Codec = Codecs.ForEmptyConstructor(() => new SelectedInEditor()).Build();
    
    public Identifier Id() => Identifier.Of(Program.Assembly, "selected_in_editor");
}