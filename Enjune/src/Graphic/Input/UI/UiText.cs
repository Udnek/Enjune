using Enjune.Graphic.Font;

namespace Enjune.Graphic.Input.UI;

public class UiText(Rect anchor, Margin margin, float z, CompiledFont font, String text, params UiElement[] children) : UiElement(anchor, margin, z, children)
{
    private readonly CompiledFont _font = font;
    public string Text = text;
    private readonly Color _color = new(RandomFloat(), RandomFloat(), RandomFloat(), 1);
    
    private static float RandomFloat() => (float) new Random().NextDouble();
    
    protected override void GenerateMeshes()
    {
        base.GenerateMeshes();
        var perMeshData = new Model.PerMesh(_font.Material, _color);
        _font.GenerateMeshes(Text, GlobalRect.Height, mesh =>
        {
            mesh.Offset(new Position(GlobalRect.Min.X, GlobalRect.Min.Y, GlobalZ+0.1f));
            Meshes.Add(new Model.Entry(mesh, perMeshData));
        });
    }
}