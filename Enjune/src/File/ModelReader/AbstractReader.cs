using Enjune.Graphic;
using Enjune.Graphic.Asset;

namespace Enjune.File.ModelReader;

public abstract class AbstractReader(AssetManager assetManager, ResourcePath path)
{
    protected readonly AssetManager AssetManager = assetManager;
    protected readonly ResourcePath Path = path;

    public abstract Model? Read(out Error? error);
}