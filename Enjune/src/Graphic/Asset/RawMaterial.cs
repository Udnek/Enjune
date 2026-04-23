using Enjune.File;

namespace Enjune.Graphic.Asset;

public record RawMaterial
{
    public string Name;
    public ResourcePath? TexturePath;
    public ByteImage? LoadedTexture; // more prior
    public Color Color = (1,1,1,1);

    public RawMaterial(string name) => Name = name;

    public static RawMaterial FromTexture(ResourcePath texturePath) 
        => new(texturePath.ToString()){TexturePath = texturePath};
    public static RawMaterial FromTexture(ByteImage texture, string name) 
        => new(name){LoadedTexture = texture};
    public static RawMaterial FromColor(Color color, string name) 
        => new(name){Color = color};
    public static RawMaterial White(string name) 
        => new RawMaterial(name);
}