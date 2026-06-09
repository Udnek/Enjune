using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace Enjune.Graphic.UI;

public abstract class UiBaseElement : UiElement
{
    private Rect LocalAnchor { get; }
    private Margin Margin { get; }

    public override bool IsHovered
    {
        get;
        set
        {
            if (field && !value) OnCursorExit();
            else if (!field && value) OnCursorEnter();
            field = value;
        }
    } = false;

    private Rect _globalAnchorDebugOnly;
    
    protected UiBaseElement(Rect anchor, Margin margin, float z, params UiElement[] children) : base(
        children.Length == 0 ? null : children, new List<Model.Entry>(1))
    {
        GlobalZ = z;
        LocalAnchor = anchor;
        Margin = margin;
    }

    protected virtual void OnCursorEnter(){}

    protected virtual void OnCursorExit(){}
    
    public override void UpdateOnlySelfRect(Rect parentRect)
    {
        var glAnchor = new Rect(
            parentRect.Min + parentRect.Size*LocalAnchor.Min,
            parentRect.Min + parentRect.Size*LocalAnchor.Max);
        
        _globalAnchorDebugOnly = glAnchor;
        
        GlobalRect = new Rect(
            (glAnchor.Min.X + Margin.Left, glAnchor.Min.Y + Margin.Bottom),
            (glAnchor.Max.X - Margin.Right, glAnchor.Max.Y - Margin.Top)
        );
    }
    
    public override void RegenerateSelfMeshes()
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
            minAnchor.Offset(new Vector3(_globalAnchorDebugOnly.Min));
            minAnchor.Offset((0, 0, GlobalZ+5));
            Meshes.Add(new Model.Entry(minAnchor, new Model.PerMesh(color)));
        }
        {
            var maxAnchor = Mesh.Triangle((0f, 0, 0), (1f, 0.5f, 0), (0.5f, 1, 0), TextureQuad.Full);
            maxAnchor.Multiply(new Vector3(anchorSize)); // just resizing to be visible on screen;
            maxAnchor.Offset(new Vector3(_globalAnchorDebugOnly.Max));
            maxAnchor.Offset((0, 0, GlobalZ+5));
            Meshes.Add(new Model.Entry(maxAnchor, new Model.PerMesh(color)));
        }
    }
}