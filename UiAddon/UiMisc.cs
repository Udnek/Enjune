using System.Diagnostics.Contracts;

namespace UiAddon;

public readonly record struct Margin(float Right, float Top, float Left, float Bottom)
{
    public static readonly Margin No = new(0, 0, 0, 0);
    public static Margin Inside(float m) => new(m, m, m, m);
    public static Margin Outside(float m) => Inside(-m);
    public static Margin Inside(float x, float y) => new(x, y, x, y);
    public static Margin Outside(float x, float y) => Inside(-x, -y);

    [Pure]
    public Margin Move(float x, float y) => new(Right - x, Top - y, Left + x, Bottom + y);
}

public readonly record struct Rect
{
    public Rect(Vector2 firstPoint, Vector2 secondPoint)
    {
        Min = Vector2.ComponentMin(firstPoint, secondPoint);
        Max = Vector2.ComponentMax(firstPoint, secondPoint);
    }

    public Vector2 Size => Max - Min;
    public float Height => Max.Y - Min.Y;
    public float Width => Max.X - Min.X;
    public Vector2 Min { get; init; }
    public Vector2 Max { get; init; }

    public bool ContainsPoint(Vector2 point) 
        => (Min.X <= point.X && Min.Y <= point.Y) && (point.X <= Max.X && point.Y <= Max.Y);
    
    public bool FullyContains(Rect other) 
        => (Min.X <= other.Min.X && Min.Y <= other.Min.Y) && (other.Max.X <= Max.X && other.Max.Y <= Max.Y);
    
    [Pure]
    public Rect Move(float x, float y) => new(Min+(x, y), Max+(x, y));
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

    public static Rect CalculateRectFromParent(Rect parent, Rect anchor, Margin margin)
    {
        var rectWithoutMargin = new Rect(
            parent.Min + parent.Size * anchor.Min,
            parent.Min + parent.Size * anchor.Max);

        return new Rect(
            (rectWithoutMargin.Min.X + margin.Left, rectWithoutMargin.Min.Y + margin.Bottom), 
            (rectWithoutMargin.Max.X - margin.Right, rectWithoutMargin.Max.Y - margin.Top));
    }
}