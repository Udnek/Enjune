namespace Enjune.Graphic.Asset;

public record CompiledAssets(
    int TextureSize, 
    List<ByteImage> Textures, 
    CompiledMaterial[] Materials
    )
{
}