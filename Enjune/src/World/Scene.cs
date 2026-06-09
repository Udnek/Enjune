using Enjune.Data;

namespace Enjune.World;

public class Scene
{
    public static readonly Codec<Scene> Codec = Codecs
        .ForEmptyConstructor(() => new Scene())
        .ForField("objects", 
            i => i.Objects.Where(o => o.ToBeSerialized).ToArray(), 
            (ref i, objs) => i.Objects = objs.ToList(), SObject.Codec.Array, [])
        .Build();

    public List<SObject> Objects { get; private set; } = [];
}