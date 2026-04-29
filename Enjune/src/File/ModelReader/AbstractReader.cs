using Enjune.Graphic;
using Enjune.Graphic.Asset;

namespace Enjune.File.ModelReader;

public abstract class AbstractReader
{
    protected readonly AssetManager AssetManager;
    protected readonly ResourcePath Path;

    public AbstractReader(AssetManager assetManager, ResourcePath path)
    {
        AssetManager = assetManager;
        Path = path;
    }

    public abstract MaterialModel? Read(out Error? error);
}