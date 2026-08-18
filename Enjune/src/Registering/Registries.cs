using Enjune.Data.Codec;
using Enjune.Ecs.Component;

namespace Enjune.Registering;

public static class Registries
{
    public static readonly WritableRegistry<IRegistry<object>> All = 
        WritableRegistry<IRegistry<object>>.CreateRootRegistry(Identifier.Of(Enjune.Assembly, "root"));
    public static readonly WritableRegistry<IObjCodec> Codec = 
        WritableRegistry<IObjCodec>.CreateAndRegister(Identifier.Of(Enjune.Assembly, "codec"));
    public static readonly WritableRegistry<Type> EcsComponentType = 
        WritableRegistry<Type>.CreateAndRegister(Identifier.Of(Enjune.Assembly, "ecs_component"));
}