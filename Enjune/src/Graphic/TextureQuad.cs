namespace Enjune.Graphic;

public readonly record struct TextureQuad(
    TexturePos TopLeft,
    TexturePos TopRight,
    TexturePos BotLeft,
    TexturePos BotRight)
{
    public static TextureQuad FromCorners(TexturePos firstCorner, TexturePos secondCorner)
    {
        var min = Vector2.ComponentMin(firstCorner, secondCorner);
        var max = Vector2.ComponentMax(firstCorner, secondCorner);
        return new TextureQuad
        {
            TopLeft = (min.X, max.Y),
            TopRight = max,
            BotLeft = min,
            BotRight = (max.X, min.Y)
        };
    }

    public static readonly TextureQuad Full = FromCorners((0, 0), (1, 1));
    
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