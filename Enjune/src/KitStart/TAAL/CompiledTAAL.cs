using Enjune.Graphic;

namespace Enjune.KitStart.TAAL;

/// <summary>
/// TAAL stand for Texture Array Asset Loader
/// </summary>
public record CompiledTAAL (
    MatIdx MissingMaterialIndex,
    int TextureSize, // NxN 
    ByteImage[] Textures, 
    TAALData.Material[] Materials);