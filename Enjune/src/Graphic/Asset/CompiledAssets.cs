namespace Enjune.Graphic.Asset;

public record CompiledAssets(
    int TextureSize, 
    List<byte[]> Textures, 
    CompiledMaterial[] Materials
    )
{
}