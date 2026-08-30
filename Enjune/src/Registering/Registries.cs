using Enjune.Data.Codec;

namespace Enjune.Registering;

public static class Registries
{
    public static readonly WritableRegistry<IRegistry<object>> All = 
        WritableRegistry<IRegistry<object>>.CreateRootRegistry(Identifier.Of(Enjune.Assembly, "root"));
    public static readonly WritableRegistry<IObjCodec> Codec = 
        WritableRegistry<IObjCodec>.CreateAndRegister(Identifier.Of(Enjune.Assembly, "codec"));
}