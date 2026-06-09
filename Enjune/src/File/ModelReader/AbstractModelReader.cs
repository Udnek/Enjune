using Enjune.Data;
using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Modeling;

namespace Enjune.File.ModelReader;

public abstract class AbstractModelReader
{
    public static readonly Codec<AbstractModelReader> Codec = Codecs
        .ForEither<AbstractModelReader>()
        .OrIfInstance(".obj", Codecs.ForEmptyConstructor(() => new DotObjReader()).Build())
        .OrIfInstance(".map", Codecs.ForEmptyConstructor(() => new DotMapReader()).Build())
        .OrIfInstance(".glb", Codecs.ForEmptyConstructor(() => new DotGlbReader()).Build())
        .Build();
    
    protected AssetManager AssetManager = null!;
    protected ResourcePath Path = null!;

    public Model? Read(AssetManager assetManager, ResourcePath path, out Error? error)
    {
        AssetManager = assetManager;
        Path = path;
        return Read(out error);
    }

    protected abstract Model? Read(out Error? error);
}