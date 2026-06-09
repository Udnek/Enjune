using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace Enjune.Graphic.UI;

public class UiText(Rect anchor, Margin margin, float z, CompiledFont font, string text, Color? color = null, params UiElement[] children) : UiBaseElement(anchor, margin, z, children)
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private readonly CompiledFont _font = font;
    public string Text = text;
    private readonly Color _color = color ?? new Color(RandomFloat(), RandomFloat(), RandomFloat(), 1);
    
    // TODO remove
    private static float RandomFloat() => (float) new Random().NextDouble();
    
    protected override void GenerateMeshes()
    {
        base.GenerateMeshes();
        
        var perMeshData = new Model.PerMesh(_font.Material, _color);
        var split = Text.Split('\n');
        for (var i = 0; i < split.Length; i++)
        {
            var line = split[i];
            _font.GenerateMeshes(line, GlobalRect.Height, mesh =>
            {
                mesh.Offset(new Position(GlobalRect.Min.X, GlobalRect.Min.Y - i*GlobalRect.Height, GlobalZ + 0.1f));
                Meshes.Add(new Model.Entry(mesh, perMeshData));
            });
        }
    }
}