namespace Enjune.Graphic.UI;

public readonly record struct Margin(float Right, float Top, float Left, float Bottom)
{
    public static readonly Margin No = new(0, 0, 0, 0);
    public static Margin Inside(float m) => new(m, m, m, m);
    public static Margin Outside(float m) => Inside(-m);
    public static Margin Inside(float x, float y) => new(x, y, x, y);
    public static Margin Outside(float x, float y) => Inside(-x, -y);
}

public readonly record struct Rect(Vector2 Min, Vector2 Max)
{
    public Vector2 Size => Max - Min;
    public float Height => Max.Y - Min.Y;
    public float Width => Max.X - Min.X;

    public bool IsPointIn(Vector2 point) =>
        (Min.X <= point.X && Min.Y <= point.Y) && (point.X <= Max.X && point.Y <= Max.X);
}

public static class Anchor
{
    public static readonly Vector2 Stretch = new(0, 1);
    public static Rect OfMinMax(Vector2 min, Vector2 max) => new(min, max);
    public static Rect OfXy(Vector2 x, Vector2 y) => OfMinMax((x.X, y.X), (x.Y, y.Y));

    public static readonly Rect FixedAtCenter = FixedAt(0.5f, 0.5f);
    public static Rect FixedAt(Vector2 pos) => new(pos, pos);
    public static Rect FixedAt(float x, float y) => FixedAt((x, y));
    
    public static readonly Rect FullStretch = new(Vector2.Zero, Vector2.One);
    public static Rect StretchWithMarginInside(float m) => new((m, m), (1-m, 1-m));
}