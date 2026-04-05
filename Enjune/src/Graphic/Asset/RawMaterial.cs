using Enjune.File;

namespace Enjune.Graphic.Asset;

public record RawMaterial
{
    public ResourcePath? TexturePath;
    public Color Color = (1,1,1,1);

    public RawMaterial(){}
    
    public static RawMaterial FromTexture(ResourcePath texturePath) => new(){TexturePath = texturePath};
    public static RawMaterial FromColor(Color color) => new(){Color = color};
    public static RawMaterial White() => new RawMaterial();
}