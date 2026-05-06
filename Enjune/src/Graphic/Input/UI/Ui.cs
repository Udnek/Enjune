using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Enjune.Graphic.Font;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Input.UI;

public record struct Margin(float Right, float Top, float Left, float Bottom)
{
    public static readonly Margin No = new(0, 0, 0, 0);
    public static Margin Inside(float m) => new(m, m, m, m);
    public static Margin Outsize(float m) => Inside(-m);
}

public record struct Rect(Vector2 Min, Vector2 Max)
{
    public Vector2 Size => Max - Min;
}

public static class Anchor
{
    public static readonly Rect FixedAtCenter = FixedAt(0.5f, 0.5f);
    public static Rect FixedAt(Vector2 pos) => new(pos, pos);
    public static Rect FixedAt(float x, float y) => FixedAt((x, y));
    
    public static readonly Rect Stretch = new(Vector2.Zero, Vector2.One);
    public static Rect StretchWithMarginInside(float m) => new((m, m), (1-m, 1-m));
}

public sealed class Ui(params UiElement[] roots)
{
    private readonly List<UiElement> _roots = roots.ToList();
    
    public readonly Matrix4 ModelTransform = Matrix4.Identity;
    public readonly Matrix4 ViewTransform = Matrix4.Identity;
    public Matrix4 ProjectionTransform => Matrix4.CreateOrthographicOffCenter(0, Size.X, 0, Size.Y, -10, 10);

    public Vector2 Size = (1, 1);
    public float PixelsPerUnit = 1;

    public MaterialModel UpdateAndCreateModel()
    {
        var rect = new Rect((0, 0), (Size.X, Size.Y));
        _roots.ForEach(ch => ch.Update(rect, PixelsPerUnit));
        
        var builder = new MaterialModel.Builder();
        _roots.ForEach(ch => ch.CreateMeshes(m => builder.Add(m, Color.One)));
        return builder.Build();
    }

    public void LogHierarchy()
    {
        LogWithDepth(0, $"{GetType().Name}: size={Size}; pixelsPerUnit={PixelsPerUnit};");
        _roots.ForEach(ch => ch.LogHierarchy(1));
    }
    
    public static void LogWithDepth(int depth, object? message) 
        => Logger.Log(typeof(Ui), new string(' ', depth*2) + message);
}

public abstract class UiElement(Rect anchor, Margin margin, float z, params UiElement[] children)
{
    public readonly float GlobalZ = z;
    public Rect LocalAnchor { get; protected set; } = anchor;
    public Margin Margin { get; protected set; } = margin;
    public Rect GlobalRect { get; protected set; }

    protected Rect GlobalAnchor_DebugOnly;

    protected readonly List<UiElement>? Children = children.Length == 0 ? null : [..children];

    public void Update(UiElement parent, float pixelsPerUnit)
        => Update(parent.GlobalRect, pixelsPerUnit);
    
    public virtual void Update(Rect parentRect, float pixelsPerUnit)
    {
        var glAnchor = new Rect(
            parentRect.Min + parentRect.Size*LocalAnchor.Min,
            parentRect.Min + parentRect.Size*LocalAnchor.Max);
        
        GlobalAnchor_DebugOnly = glAnchor;
        
        GlobalRect = new Rect(
            (glAnchor.Min.X + Margin.Left*pixelsPerUnit, glAnchor.Min.Y + Margin.Bottom*pixelsPerUnit),
            (glAnchor.Max.X - Margin.Right*pixelsPerUnit, glAnchor.Max.Y - Margin.Top*pixelsPerUnit)
        );
        Children?.ForEach(ch => ch.Update(this, pixelsPerUnit));
    }

    public virtual void LogHierarchy(int depth)
    {
        Ui.LogWithDepth(depth, $"{GetType().Name}: globalRect={GlobalRect}; anchor={LocalAnchor}; margin={Margin};");
        Children?.ForEach(ch => ch.LogHierarchy(depth+1));
    }

    public virtual void CreateMeshes(Consumer<Mesh<(Vector2 texCoord, Vector3 normal)>> consumer)
    {
        float anchorSize = MathF.Max(10, MathF.Sqrt(GlobalRect.Size.X + GlobalRect.Size.Y));
        var c = Color.One;
        {
            var minAnchor = Mesh.Triangle((0.5f, 0, 0), (1, 1, 0), (0, 0.5f, 0), c, c, c);
            minAnchor.Offset((-1, -1, 0));
            minAnchor.Multiply(new Vector3(anchorSize)); // just resizing to be visible on screen;
            minAnchor.Offset(new Vector3(GlobalAnchor_DebugOnly.Min));
            minAnchor.Offset((0, 0, GlobalZ+5));
            consumer(minAnchor);
        }
        {
            var maxAnchor = Mesh.Triangle((0f, 0, 0), (1f, 0.5f, 0), (0.5f, 1, 0), c, c, c);
            maxAnchor.Multiply(new Vector3(anchorSize)); // just resizing to be visible on screen;
            maxAnchor.Offset(new Vector3(GlobalAnchor_DebugOnly.Max));
            maxAnchor.Offset((0, 0, GlobalZ+5));
            consumer(maxAnchor);
        }
        
        Children?.ForEach(ch => ch.CreateMeshes(consumer));
    }
}

public sealed class UiDirectory(Rect anchor, Margin margin, float z, params UiElement[] children) : UiElement(anchor, margin, z, children);

public class UiButton(Rect anchor, Margin margin, float z, params UiElement[] children) : UiElement(anchor, margin, z, children)
{
    private readonly Color _color = new(RandomFloat(), RandomFloat(), RandomFloat(), 1);
    
    private static float RandomFloat() => (float) new Random().NextDouble();

    public override void CreateMeshes(Consumer<Mesh<Color>> consumer)
    {
        base.CreateMeshes(consumer);

        var min = new Vector3(GlobalRect.Min.X, GlobalRect.Min.Y, GlobalZ);
        var max = new Vector3(GlobalRect.Max.X, GlobalRect.Max.Y, GlobalZ);
        consumer(Mesh.Quad(
            min, (max.X, min.Y, GlobalZ), max, (min.X, max.Y, GlobalZ),
            _color, _color, _color, _color)
        );
    }
}

public class UiText(Rect anchor, Margin margin, float z, CompiledFont font, String text, params UiElement[] children) : UiElement(anchor, margin, z, children)
{
    private readonly Color _color = new(RandomFloat(), RandomFloat(), RandomFloat(), 1);
    
    private static float RandomFloat() => (float) new Random().NextDouble();

    public override void CreateMeshes(Consumer<Mesh<Color>> consumer)
    {
        base.CreateMeshes(consumer);

        var min = new Vector3(GlobalRect.Min.X, GlobalRect.Min.Y, GlobalZ);
        var max = new Vector3(GlobalRect.Max.X, GlobalRect.Max.Y, GlobalZ);
        
        consumer(Mesh.Quad(
            min, (max.X, min.Y, GlobalZ), max, (min.X, max.Y, GlobalZ),
            _color, _color, _color, _color)
        );
    }
}