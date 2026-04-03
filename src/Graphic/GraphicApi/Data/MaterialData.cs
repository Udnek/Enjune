using System.Runtime.InteropServices;

namespace Enjune.Graphic.GraphicApi;

[StructLayout(LayoutKind.Sequential)]
public record struct MaterialData(
    Color Color, 
    TexId TextureId
    );