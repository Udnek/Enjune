using Enjune.Data.Codec;
using Enjune.File;
using Enjune.KitStart.Modeling;
using Enjune.KitStart.TAAL;

namespace Enjune.KitStart.ModelLoader;

public abstract class AbstractModelReader
{
    public static readonly InstanceMatchCodec<AbstractModelReader> Codec = Codecs
        .ForMatchInstance<AbstractModelReader>()
        .IfInstance(".obj", Codecs.ForEmptyConstructor(() => new DotObjReader()).Build())
        .IfInstance(".map", Codecs.ForEmptyConstructor(() => new DotMapReader()).Build())
        .IfInstance(".glb", Codecs.ForEmptyConstructor(() => new DotGlbReader()).Build())
        .Build();
    
    protected TextureArrayAssetLoader TextureArrayAssetLoader = null!;
    protected ResourcePath Path = null!;

    public StaticModel<TAALData.Vertex, TAALData.PerPrimitive>? Read(TextureArrayAssetLoader textureArrayAssetLoader, ResourcePath path, out Error? error)
    {
        TextureArrayAssetLoader = textureArrayAssetLoader;
        Path = path;
        return Read(out error);
    }

    protected abstract StaticModel<TAALData.Vertex, TAALData.PerPrimitive>? Read(out Error? error);
}