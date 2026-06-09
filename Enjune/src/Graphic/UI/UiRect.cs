using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace Enjune.Graphic.UI;

public class UiRect(Rect anchor, Margin margin, float z, Color? color = null, params UiElement[] children)
    : UiBaseElement(anchor, margin, z, children)
{
    private Color _color = color ?? new Color(RandomFloat(), RandomFloat(), RandomFloat(), 1);

    // TODO remove
    private static float RandomFloat() => (float)new Random().NextDouble();

    protected override void OnCursorEnter()
    {
        Logger.Highlight(this, "enter");
        _color *= 1.5f;
    }

    protected override void OnCursorExit()
    {
        Logger.Highlight(this, "exit");
        _color /= 1.5f;
    }
    
    protected override void GenerateMeshes()
    {
        base.GenerateMeshes();
        var min = new Vector3(GlobalRect.Min.X, GlobalRect.Min.Y, GlobalZ);
        var max = new Vector3(GlobalRect.Max.X, GlobalRect.Max.Y, GlobalZ);
        Meshes.Add(new Model.Entry(
            Mesh.Quad(
                min, (max.X, min.Y, GlobalZ), max, (min.X, max.Y, GlobalZ),
                TextureQuad.Full),
            new Model.PerMesh(_color)
            )
        );
    }
}