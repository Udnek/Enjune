using Enjune.Misc;

namespace Enjune.Graphic.UI;

public abstract class UiElement(Rect anchor, Margin margin, float z, params UiElement[] children)
{
    public readonly float GlobalZ = z;
    public Rect LocalAnchor { get; protected set; } = anchor;
    public Margin Margin { get; protected set; } = margin;
    public Rect GlobalRect { get; protected set; }
    public bool LocalHidden = false;
    public List<Model.Entry> Meshes = [];

    protected Rect GlobalAnchor_DebugOnly;

    public readonly UiElement[]? Children = children.Length == 0 ? null : children;
    
    // updates rect only if provided parent rect
    public virtual void UpdateAndRegenerateMeshes(Rect? parentRect)
    {
        if (parentRect.HasValue)
        {
            var parRect = parentRect.Value;
            var glAnchor = new Rect(
                parRect.Min + parRect.Size*LocalAnchor.Min,
                parRect.Min + parRect.Size*LocalAnchor.Max);
        
            GlobalAnchor_DebugOnly = glAnchor;
        
            GlobalRect = new Rect(
                (glAnchor.Min.X + Margin.Left, glAnchor.Min.Y + Margin.Bottom),
                (glAnchor.Max.X - Margin.Right, glAnchor.Max.Y - Margin.Top)
            );
        }
        
        UpdateMeshes();
    }
    
    public void UpdateMeshes()
    {
        Meshes.Clear();
        GenerateMeshes();
    }

    protected virtual void GenerateMeshes()
    {
        var anchorSize = MathF.Max(10, MathF.Sqrt(GlobalRect.Size.X + GlobalRect.Size.Y));
        var color = Color.One;
        {
            var minAnchor = Mesh.Triangle((0.5f, 0, 0), (1, 1, 0), (0, 0.5f, 0), TextureQuad.Full);
            minAnchor.Offset((-1, -1, 0));
            minAnchor.Multiply(new Vector3(anchorSize)); // just resizing to be visible on screen;
            minAnchor.Offset(new Vector3(GlobalAnchor_DebugOnly.Min));
            minAnchor.Offset((0, 0, GlobalZ+5));
            Meshes.Add(new Model.Entry(minAnchor, new Model.PerMesh(color)));
        }
        {
            var maxAnchor = Mesh.Triangle((0f, 0, 0), (1f, 0.5f, 0), (0.5f, 1, 0), TextureQuad.Full);
            maxAnchor.Multiply(new Vector3(anchorSize)); // just resizing to be visible on screen;
            maxAnchor.Offset(new Vector3(GlobalAnchor_DebugOnly.Max));
            maxAnchor.Offset((0, 0, GlobalZ+5));
            Meshes.Add(new Model.Entry(maxAnchor, new Model.PerMesh(color)));
        }
    }
    
    public virtual void LogHierarchyRecursively(int depth)
    {
        Ui.LogWithDepth(depth, $"{GetType().Name}: globalRect={GlobalRect}; anchor={LocalAnchor}; margin={Margin};");
        Children?.ForEach(ch => ch.LogHierarchyRecursively(depth+1));
    }
}