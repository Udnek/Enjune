using Enjune.Registering;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs;

public class Components
{
    public static void Boot()
    {
        Registries.Codec.Register(new Transform().Id(), Transform.Codec);
    }
}