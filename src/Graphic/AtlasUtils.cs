using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic;

public static class AtlasUtils
{
    private const int Size = 256;
    private const int UnitSize = 16;
    private const float Factor = (float) UnitSize / Size;
    
    public static TextureQuad GetAt(int x, int y)
    {
        y = (Size / UnitSize) -y -1;
        return new TextureQuad
        (
            new TextureCoord(x, y)*Factor,
            new TextureCoord(x+1, y)*Factor,
            new TextureCoord(x+1, y+1)*Factor,
            new TextureCoord(x, y+1)*Factor
        );
    }
}

public struct TextureQuad(TextureCoord botLeft, TextureCoord botRight, TextureCoord topRight, TextureCoord topLeft)
{
    public TextureCoord BotLeft = botLeft;
    public TextureCoord BotRight = botRight;
    public TextureCoord TopRight = topRight;
    public TextureCoord TopLeft = topLeft;
}