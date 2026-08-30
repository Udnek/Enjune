using Enjune.Data.Codec;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Modeling;

namespace Enjune.File.ModelReader;

public abstract class AbstractModelReader
{
    public static readonly InstanceMatchCodec<AbstractModelReader> Codec = Codecs
        .ForMatchInstance<AbstractModelReader>()
        .IfInstance(".obj", Codecs.ForEmptyConstructor(() => new DotObjReader()).Build())
        .IfInstance(".map", Codecs.ForEmptyConstructor(() => new DotMapReader()).Build())
        .IfInstance(".glb", Codecs.ForEmptyConstructor(() => new DotGlbReader()).Build())
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