namespace Enjune.Graphic.Asset;

public record CompiledAssets(
    CompiledMaterial WhiteMaterial,
    CompiledMaterial MissingMaterial,
    
    int TextureSize, 
    List<ByteImage> Textures, 
    CompiledMaterial[] Materials
    );