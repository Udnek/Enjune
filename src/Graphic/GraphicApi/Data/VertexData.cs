using System.Runtime.InteropServices;

namespace Enjune.Graphic.GraphicApi.Data;

[StructLayout(LayoutKind.Sequential)]
public record struct VertexData(
    Position Position,
    TextureCoord TextureCoord
);
