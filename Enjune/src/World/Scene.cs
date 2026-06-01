using Enjune.Data;

namespace Enjune.World;

public class Scene
{
    public static readonly Codec<Scene> Codec = Codecs.NewBuilder(() => new Scene())
        .ForField("objects", i => i.Objects.ToArray(), (ref i, objs) => i.Objects = objs.ToList(), SObject.Codec.Array)
        .Build();
    
    public List<SObject> Objects = [];
}