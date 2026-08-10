using Enjune.Data;
using Enjune.Data.Codec;

namespace Enjune.World;

public class Scene
{
    public static readonly MapCodec<Scene> Codec = Codecs
        .ForEmptyConstructor(() => new Scene())
        .ForField("objects", 
            i => i.Objects.Where(o => o.ToBeSerialized).ToArray(), 
            (ref i, objs) => i.Objects = objs.ToList(), Codecs.ArrayOf(SObject.Codec))
        .Build();

    public List<SObject> Objects { get; private set; } = [];
}