namespace Enjune.Graphic;

public readonly record struct TextureQuad(
    TextureCoord BotLeft, 
    TextureCoord BotRight, 
    TextureCoord TopRight, 
    TextureCoord TopLeft)
{
    public TextureQuad(TextureCoord botLeft, TextureCoord topRight) : this(
        botLeft,
        new TextureCoord(topRight.X, botLeft.Y),
        topRight,
        new TextureCoord(botLeft.X, topRight.Y)){}

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