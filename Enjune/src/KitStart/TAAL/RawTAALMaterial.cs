using Enjune.File;
using Enjune.Graphic;

namespace Enjune.KitStart.TAAL;

/// <summary>
/// TAAL stand for Texture Array Asset Loader
/// </summary>
public sealed record RawTAALMaterial
{
    public string Name; // debug purpose
    public ResourcePath? TexturePath; 
    public ByteImage? LoadedTexture; // more prior over TexturePath
    public Color Color = (1,1,1,1);

    private RawTAALMaterial(string name) => Name = name;

    public static RawTAALMaterial FromTexture(ResourcePath texturePath) 
        => new(texturePath.ToString()){TexturePath = texturePath};
    public static RawTAALMaterial FromTexture(ByteImage texture, string name) 
        => new(name){LoadedTexture = texture};
    public static RawTAALMaterial FromColor(Color color, string name) 
        => new(name){Color = color};
    public static RawTAALMaterial White(string name) 
        => new RawTAALMaterial(name);
}