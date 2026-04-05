using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic;

public struct TextureQuad(TextureCoord botLeft, TextureCoord botRight, TextureCoord topRight, TextureCoord topLeft)
{
    public TextureCoord BotLeft = botLeft;
    public TextureCoord BotRight = botRight;
    public TextureCoord TopRight = topRight;
    public TextureCoord TopLeft = topLeft;

    public static readonly TextureQuad Full = new TextureQuad((0, 0), (1, 0), (1, 1), (0, 1));
    public static readonly TextureQuad Tnt = GetAtAtlas(8, 0);
    public static readonly TextureQuad Furnace = GetAtAtlas(13, 3);
    public static readonly TextureQuad Planks = GetAtAtlas(4, 0);
    
    private const int Size = 256;
    private const int UnitSize = 16;
    private const float Factor = (float) UnitSize / Size;
    
    public static TextureQuad GetAtAtlas(int x, int y)
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
    
    public TextureCoord this[int index] =>
        index switch
        {
            0 => BotLeft,
            1 => BotRight,
            2 => TopRight,
            3 => TopLeft,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };
}