using Enjune.Registering;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs;

public static class Components
{
    public static void Boot()
    {
        Registries.Codec.Register(new Transform().Id(), Transform.Codec);
        Registries.Codec.Register(new SpotLightComponent().Id(), SpotLightComponent.Codec);
        Registries.Codec.Register(new StaticModelComponent().Id(), StaticModelComponent.Codec);
        Registries.Codec.Register(new SelectedInEditor().Id(), SelectedInEditor.Codec);
    }
}