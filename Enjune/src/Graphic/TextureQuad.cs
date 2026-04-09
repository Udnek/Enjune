using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic;

public struct TextureQuad(TextureCoord botLeft, TextureCoord topRight)
{
    public TextureCoord BotLeft = botLeft;
    public TextureCoord BotRight = new(topRight.X, botLeft.Y);
    public TextureCoord TopRight = topRight;
    public TextureCoord TopLeft = new(botLeft.X, topRight.Y);

    public static readonly TextureQuad Full = new((0, 0), (1, 1));
    
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