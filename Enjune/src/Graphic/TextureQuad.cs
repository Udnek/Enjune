namespace Enjune.Graphic;

public readonly record struct TextureQuad(
    TexturePos BotLeft, 
    TexturePos BotRight, 
    TexturePos TopRight, 
    TexturePos TopLeft)
{
    public TextureQuad(TexturePos botLeft, TexturePos topRight) : this(
        botLeft,
        new TexturePos(topRight.X, botLeft.Y),
        topRight,
        new TexturePos(botLeft.X, topRight.Y)){}

    public static readonly TextureQuad Full = new((0, 0), (1, 1));
    
    public TexturePos this[int index] =>
        index switch
        {
            0 => BotLeft,
            1 => BotRight,
            2 => TopRight,
            3 => TopLeft,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };
}