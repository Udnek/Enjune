using System.Reflection;
using Enjune.Data.Codec;
using Enjune.Graphic.Modeling;
using Enjune.Registering;

namespace SceneMaker.Misc;

public static class Models
{
    public static readonly Registry<Model> Registry = new();
    private static readonly ICodec<Registry<Model>> RegistryCodec = Codecs.ForEmptyConstructor(() => Registry).Build();
    public static readonly ICodec<ResourceKey<Model>> ResourceKeyCodec = ResourceKey<Model>.CreateCodec(RegistryCodec);
    

    public static readonly ResourceKey<Model> ErrorCube = Create(Program.Assembly, "error_cube");
    public static readonly ResourceKey<Model> Calavera = Create(Program.Assembly, "calavera");
    public static readonly ResourceKey<Model> WhiteCube = Create(Program.Assembly, "white_cube");
    
    public static ResourceKey<Model> Create(Assembly assembly, string name)
    {
        return new ResourceKey<Model>(Registry, Identifier.Of(assembly, name));
    }
}